using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

interface IArcSource
{
    IEnumerable<IArcModifier> ContinuationModifiers { get; }

    ArcTree.Continuation GetInitialArcParametersWithoutModifiers();
}

interface IArcModifier
{
    void Modify(ref ArcTree.Continuation arc);
}

static class ArcTree
{
    public record struct Arc(
        GameObject Source,
        GameObject Target,
        Continuation Continuation
    );

    public record struct Continuation(
        GameObject Source,
        int BouncesBefore,
        int BouncesLeft,
        int Branches,
        int MaxBounceDistance)
    {
        public static Continuation Initial(
            GameObject source, int bounces = 1, int branches = 1, int maxBounceDistance = 2)
            => new(source, 0, bounces, branches, maxBounceDistance);
    }

    public static List<Arc> Strike(
        GameState game,
        GameObject? source,
        IArcSource arcSource,
        ImmutableArray<Tile> areaInRange)
    {
        var state = setupState(game, source, arcSource);

        var initialContinuation = createInitialContinuation(state, arcSource);
        connectToTargetsInArea(state, initialContinuation, areaInRange);

        while (state.QueuedContinuations.TryDequeue(out var continuation))
        {
            var area = getAreaForContinuation(state, continuation);
            connectToTargetsInArea(state, continuation, area);
        }

        return state.Arcs;
    }

    private static State setupState(GameState game, GameObject? source, IArcSource arcSource)
    {
        var state = new State(
            game,
            game.PassabilityObserver.GetLayer(Passability.Projectile),
            arcSource.ContinuationModifiers.ToImmutableArray(),
            new FloodFillRanger());

        if (source != null)
            state.BlackList.Add(source);

        return state;
    }

    private sealed record State(
        GameState Game,
        PassabilityLayer Passability,
        ImmutableArray<IArcModifier> ArcModifiers,
        IRanger Ranger)
    {
        public Random Random { get; } = new();
        public HashSet<GameObject> BlackList { get; } = new();
        public List<Arc> Arcs { get; } = new();
        public Queue<Continuation> QueuedContinuations { get; } = new();
    }

    private static ImmutableArray<Tile> getAreaForContinuation(State state, Continuation continuation)
    {
        var origin = Level.GetTile(continuation.Source.Position);
        var tiles = state.Ranger.GetTilesInRange(
            state.Game, state.Passability, origin, 0.U(), continuation.MaxBounceDistance.U());
        return tiles;
    }

    private static void connectToTargetsInArea(State state, Continuation continuation, ImmutableArray<Tile> tiles)
    {
        var targets = findTargetsInArea(state, continuation.Branches, tiles);
        connectToTargets(state, continuation, targets);
    }

    private static List<GameObject> findTargetsInArea(State state, int maxCount, ImmutableArray<Tile> tiles)
    {
        var targets = tiles
            .SelectMany(newTargetsOnTileWithPossibleDuplicates)
            .Distinct()
            .RandomSubset(maxCount, state.Random);

        return targets;

        IEnumerable<GameObject> newTargetsOnTileWithPossibleDuplicates(Tile tile)
            => Enumerable.Concat(
                    state.Game.TargetLayer.GetObjectsOnTile(tile),
                    state.Game.ConductiveLayer.GetObjectsOnTile(tile))
                .WhereNot(state.BlackList.Contains);
    }

    private static void connectToTargets(State state, Continuation previousContinuation, List<GameObject> targets)
    {
        foreach (var target in targets)
        {
            state.BlackList.Add(target);
            var continuation = continueFrom(state, target, previousContinuation);

            if (continuation is { BouncesLeft: > 0, Branches: > 0 })
                state.QueuedContinuations.Enqueue(continuation);

            var arc = new Arc(previousContinuation.Source, target, continuation);
            state.Arcs.Add(arc);
        }
    }

    private static Continuation createInitialContinuation(State state, IArcSource arcSource)
    {
        var continuation = arcSource.GetInitialArcParametersWithoutModifiers();
        appleArcModifiers(state, ref continuation);
        return continuation;
    }

    private static Continuation continueFrom(State state, GameObject source, Continuation previousContinuation)
    {
        var continuation = new Continuation(
            source,
            previousContinuation.BouncesBefore + 1,
            previousContinuation.BouncesLeft - 1,
            previousContinuation.Branches,
            previousContinuation.MaxBounceDistance);

        appleArcModifiers(state, ref continuation);

        foreach (var conductive in source.GetComponents<IConductive>())
        {
            conductive.Conduct(ref continuation);
        }

        return continuation;
    }

    private static void appleArcModifiers(State state, ref Continuation continuation)
    {
        foreach (var modifier in state.ArcModifiers)
        {
            modifier.Modify(ref continuation);
        }
    }
}

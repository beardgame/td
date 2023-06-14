using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements;

interface IArcSource
{
    IEnumerable<IArcModifier> ContinuationModifiers { get; }

    ArcTree.ArcContinuation GetInitialArcParametersWithoutModifiers();
}

interface IArcModifier
{
    void Modify(ref ArcTree.ArcContinuation arc);
}

static class ArcTree
{
    private record struct ArcHit(
        GameObject Source,
        GameObject Target,
        ArcContinuation Continuation
    );

    public record struct ArcContinuation(
        GameObject Source,
        int BouncesBefore,
        int BouncesLeft,
        int Branches,
        int MaxBounceDistance)
    {
        public static ArcContinuation Initial(
            GameObject source, int bounces = 1, int branches = 1, int maxBounceDistance = 2)
            => new(source, 0, bounces, branches, maxBounceDistance);
    }

    public static void Strike(
        GameState game,
        GameObject? source,
        IArcSource arcSource,
        ImmutableArray<Tile> tilesInRange,
        UntypedDamage damage,
        TimeSpan lifeTime)
    {
        var state = setupState(game, source, arcSource);

        hitInitialTargets(state, arcSource, tilesInRange);

        while (state.TryGetContinuation(out var continuation))
        {
            var targets = findTargetsFromContinuation(state, continuation);
            hitTargets(state, continuation, targets);
        }

        // TODO: how should damage scale with different hit patterns
        // variables: bounces, branches, total hits, (distance? nah)
        // arcSource, as well as modifiers likely want to affect this
        // could also return a list of hits, and let the caller decide for more flexibility (except YAGNI)
        foreach (var hit in state.Arcs)
        {
            arc(source, hit);
        }
    }

    private static void arc(GameObject? originalSource, ArcHit hit)
    {
        // hit target
        // and create actual arc object
    }

    private static State setupState(GameState game, GameObject? source, IArcSource arcSource)
    {
        var state = new State(
            game,
            game.PassabilityObserver.GetLayer(Passability.Projectile),
            new HashSet<GameObject>(),
            arcSource.ContinuationModifiers.ToImmutableArray(),
            new FloodFillRanger());

        if (source != null)
            state.BlackList.Add(source);

        return state;
    }

    private static void hitInitialTargets(State state, IArcSource arcSource, ImmutableArray<Tile> tilesInRange)
    {
        var initialContinuation = arcSource.GetInitialArcParametersWithoutModifiers();
        appleArcModifiers(state, ref initialContinuation);

        var initialTargets = findTargetsInArea(state, initialContinuation.Branches, tilesInRange);
        hitTargets(state, initialContinuation, initialTargets);
    }

    private static void hitTargets(State state, ArcContinuation previousContinuation, List<GameObject> targets)
    {
        foreach (var target in targets)
        {
            state.BlackList.Add(target);
            var continuation = continueArc(state, target, previousContinuation);

            if (continuation is { BouncesLeft: > 0, Branches: > 0 })
                state.Continue(continuation);

            var hit = new ArcHit(previousContinuation.Source, target, continuation);
            state.Hit(hit);
        }
    }

    private sealed record State(
        GameState Game,
        PassabilityLayer Passability,
        HashSet<GameObject> BlackList,
        ImmutableArray<IArcModifier> ArcModifiers,
        IRanger Ranger)
    {
        public Random Random { get; } = new();
        public List<ArcHit> Arcs { get; } = new();
        private Queue<ArcContinuation> queuedContinuations { get; } = new();

        public void Continue(ArcContinuation continuation)
            => queuedContinuations.Enqueue(continuation);

        public bool TryGetContinuation(out ArcContinuation continuation)
            => queuedContinuations.TryDequeue(out continuation);

        public void Hit(ArcHit hit)
            => Arcs.Add(hit);
    }

    private static List<GameObject> findTargetsFromContinuation(
        State state, ArcContinuation continuation)
    {
        var origin = Level.GetTile(continuation.Source.Position);

        var tiles = state.Ranger.GetTilesInRange(
            state.Game, state.Passability, origin, 0.U(), continuation.MaxBounceDistance.U());

        var targets = findTargetsInArea(state, continuation.Branches, tiles);

        return targets;
    }

    private static List<GameObject> findTargetsInArea(
        State state, int maxCount, ImmutableArray<Tile> tiles)
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

    private static ArcContinuation continueArc(State state, GameObject source, ArcContinuation previousContinuation)
    {
        var continuation = new ArcContinuation(
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

    private static void appleArcModifiers(State state, ref ArcContinuation continuation)
    {
        foreach (var modifier in state.ArcModifiers)
        {
            modifier.Modify(ref continuation);
        }
    }
}

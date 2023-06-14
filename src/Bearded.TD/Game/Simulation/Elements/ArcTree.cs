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
        var context = setupContext(game, source, arcSource);

        hitInitialTargets(context, arcSource, tilesInRange);

        while (context.TryGetContinuation(out var continuation))
        {
            var targets = findTargetsFromContinuation(context, continuation);
            hitTargets(context, continuation, targets);
        }

        // TODO: how should damage scale with different hit patterns
        // variables: bounces, branches, total hits, (distance? nah)
        // arcSource, as well as modifiers likely want to affect this
        // could also return a list of hits, and let the caller decide for more flexibility (except YAGNI)
        foreach (var hit in context.Arcs)
        {
            arc(source, hit);
        }
    }

    private static void arc(GameObject? originalSource, ArcHit hit)
    {
        // hit target
        // and create actual arc object
    }

    private static Context setupContext(GameState game, GameObject? source, IArcSource arcSource)
    {
        var context = new Context(
            game,
            game.PassabilityObserver.GetLayer(Passability.Projectile),
            new HashSet<GameObject>(),
            arcSource.ContinuationModifiers.ToImmutableArray(),
            new FloodFillRanger());

        if (source != null)
            context.BlackList.Add(source);

        return context;
    }

    private static void hitInitialTargets(Context context, IArcSource arcSource, ImmutableArray<Tile> tilesInRange)
    {
        var initialContinuation = arcSource.GetInitialArcParametersWithoutModifiers();
        appleArcModifiers(context, ref initialContinuation);

        var initialTargets = findTargetsInArea(context, initialContinuation.Branches, tilesInRange);
        hitTargets(context, initialContinuation, initialTargets);
    }

    private static void hitTargets(Context context, ArcContinuation previousContinuation, List<GameObject> targets)
    {
        foreach (var target in targets)
        {
            context.BlackList.Add(target);
            var continuation = continueArc(context, target, previousContinuation);

            if (continuation is { BouncesLeft: > 0, Branches: > 0 })
                context.Continue(continuation);

            var hit = new ArcHit(previousContinuation.Source, target, continuation);
            context.Hit(hit);
        }
    }

    private sealed record Context(
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
        Context context, ArcContinuation continuation)
    {
        var origin = Level.GetTile(continuation.Source.Position);

        var tiles = context.Ranger.GetTilesInRange(
            context.Game, context.Passability, origin, 0.U(), continuation.MaxBounceDistance.U());

        var targets = findTargetsInArea(context, continuation.Branches, tiles);

        return targets;
    }

    private static List<GameObject> findTargetsInArea(
        Context context, int maxCount, ImmutableArray<Tile> tiles)
    {
        var targets = tiles
            .SelectMany(newTargetsOnTileWithPossibleDuplicates)
            .Distinct()
            .RandomSubset(maxCount, context.Random);

        return targets;

        IEnumerable<GameObject> newTargetsOnTileWithPossibleDuplicates(Tile tile)
            => Enumerable.Concat(
                    context.Game.TargetLayer.GetObjectsOnTile(tile),
                    context.Game.ConductiveLayer.GetObjectsOnTile(tile))
                .WhereNot(context.BlackList.Contains);
    }

    private static ArcContinuation continueArc(Context context, GameObject source, ArcContinuation previousContinuation)
    {
        var continuation = new ArcContinuation(
            source,
            previousContinuation.BouncesBefore + 1,
            previousContinuation.BouncesLeft - 1,
            previousContinuation.Branches,
            previousContinuation.MaxBounceDistance);

        appleArcModifiers(context, ref continuation);

        foreach (var conductive in source.GetComponents<IConductive>())
        {
            conductive.Conduct(ref continuation);
        }

        return continuation;
    }

    private static void appleArcModifiers(Context context, ref ArcContinuation continuation)
    {
        foreach (var modifier in context.ArcModifiers)
        {
            modifier.Modify(ref continuation);
        }
    }
}

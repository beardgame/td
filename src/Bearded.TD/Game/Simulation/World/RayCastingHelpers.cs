using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.World.RayCastResultType;

namespace Bearded.TD.Game.Simulation.World;

enum RayCastResultType
{
    HitNothing = 1,
    HitLevel = 2,
    HitObject = 4
}

readonly record struct RayCastResult(
    RayCastResultType Type,
    float RayFactor,
    Position3 Point,
    GameObject? Object,
    Direction? LastTileStep,
    Difference3? Normal);

static class RayCastingHelpers
{
    public static IEnumerable<RayCastResult> CastPiercingRayAgainstObjects(
        this Level level, Ray3 ray, ObjectLayer objectLayer, PassabilityLayer passabilityLayer)
    {
        return CastPiercingRayAgainstObjects(level, ray, objectLayer, t => passabilityLayer[t].IsPassable);
    }

    public static IEnumerable<RayCastResult> CastPiercingRayAgainstObjects(
        this Level level, Ray3 ray, ObjectLayer objectLayer, Predicate<Tile> isPassableCheck)
    {
        level.Cast(ray.XY, out var rayCaster);

        while (rayCaster.MoveNext(out var tile))
        {
            if (!level.IsValid(tile) || !isPassableCheck(tile))
            {
                var factor = rayCaster.CurrentRayFactor;
                yield return new RayCastResult(HitLevel, factor, ray.PointAt(factor), null, rayCaster.LastStep, null);
                yield break;
            }

            var objects = objectLayer.GetObjectsOnTile(tile);
            var hits = new List<HitCandidate>();

            foreach (var obj in objects)
            {
                if (obj.TryGetSingleComponent<ICollider>(out var collider)
                    && collider.TryHit(ray, out var f, out var point, out var normal))
                {
                    hits.Add(new HitCandidate(obj, f, point, normal));
                }
            }

            hits.Sort(HitCandidate.FactorComparison);
            foreach (var hit in hits)
            {
                yield return hit.ToHitResult(rayCaster.LastStep);
            }
        }

        yield return new RayCastResult(HitNothing, 1, ray.PointAtEnd, null, rayCaster.LastStep, null);
    }

    public static RayCastResult CastRayAgainstObjects(
        this Level level, Ray3 ray, ObjectLayer objectLayer, PassabilityLayer passabilityLayer)
    {
        return CastRayAgainstObjects(level, ray, objectLayer, t => passabilityLayer[t].IsPassable);
    }

    public static RayCastResult CastRayAgainstObjects(
        this Level level, Ray3 ray, ObjectLayer objectLayer, Predicate<Tile> isPassableCheck)
    {
        level.Cast(ray.XY, out var rayCaster);

        while (rayCaster.MoveNext(out var tile))
        {
            if (!level.IsValid(tile) || !isPassableCheck(tile))
            {
                var factor = rayCaster.CurrentRayFactor;
                return new RayCastResult(HitLevel, factor, ray.PointAt(factor), null, rayCaster.LastStep, null);
            }

            var objects = objectLayer.GetObjectsOnTile(tile);

            var closestHit = HitCandidate.Initial;

            foreach (var obj in objects)
            {
                if (obj.TryGetSingleComponent<ICollider>(out var collider)
                    && collider.TryHit(ray, out var f, out var point, out var normal) && f < closestHit.Factor)
                {
                    closestHit = new HitCandidate(obj, f, point, normal);
                }
            }

            if (closestHit.Object != null)
            {
                return closestHit.ToHitResult(rayCaster.LastStep);
            }
        }

        return new RayCastResult(HitNothing, 1, ray.PointAtEnd, null, rayCaster.LastStep, null);
    }

    private readonly record struct HitCandidate(GameObject? Object, float Factor, Position3 Point, Difference3 Normal)
    {
        public static readonly Comparison<HitCandidate> FactorComparison =
            (left, right) => left.Factor.CompareTo(right.Factor);

        public static HitCandidate Initial => new() { Factor = float.PositiveInfinity };

        public RayCastResult ToHitResult(Direction? lastTileStep)
        {
            if (Object is null)
            {
                throw new InvalidOperationException(
                    "Can only convert a hit candidate to a hit result if the object is set.");
            }
            return new RayCastResult(HitObject, Factor, Point, Object, lastTileStep, Normal);
        }
    }
}

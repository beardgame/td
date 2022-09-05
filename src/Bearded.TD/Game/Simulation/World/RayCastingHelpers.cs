using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.World.RayCastResultType;

namespace Bearded.TD.Game.Simulation.World;

enum RayCastResultType
{
    HitNothing = 1,
    HitLevel = 2,
    HitEnemy = 4
}

readonly record struct RayCastResult(
    RayCastResultType Type, float RayFactor, Position3 Point,
    GameObject? Enemy, Direction? LastTileStep, Difference3? Normal);

static class RayCastingHelpers
{
    public static IEnumerable<RayCastResult> CastPiercingRayAgainstEnemies(
        this Level level, Ray3 ray, UnitLayer unitLayer, PassabilityLayer passabilityLayer)
    {
        return CastPiercingRayAgainstEnemies(level, ray, unitLayer, t => passabilityLayer[t].IsPassable);
    }

    public static IEnumerable<RayCastResult> CastPiercingRayAgainstEnemies(
        this Level level, Ray3 ray, UnitLayer unitLayer, Predicate<Tile> isPassableCheck)
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

            var enemies = unitLayer.GetUnitsOnTile(tile);
            var hits = new List<(GameObject unit, float factor, Position3 point, Difference3 normal)>();

            foreach (var enemy in enemies)
            {
                if (enemy.TryGetSingleComponent<ICollider>(out var collider)
                    && collider.TryHit(ray, out var f, out var point, out var normal))
                {
                    hits.Add((enemy, f, point, normal));
                }
            }

            hits.Sort((left, right) => left.factor.CompareTo(right.factor));
            foreach (var (unit, factor, point, normal) in hits)
            {
                yield return new RayCastResult(HitEnemy, factor, point, unit, rayCaster.LastStep, normal);
            }
        }

        yield return new RayCastResult(HitNothing, 1, ray.PointAtEnd, null, rayCaster.LastStep, null);
    }

    public static RayCastResult CastRayAgainstEnemies(
        this Level level, Ray3 ray, UnitLayer unitLayer, PassabilityLayer passabilityLayer)
    {
        return CastRayAgainstEnemies(level, ray, unitLayer, t => passabilityLayer[t].IsPassable);
    }

    public static RayCastResult CastRayAgainstEnemies(
        this Level level, Ray3 ray, UnitLayer unitLayer, Predicate<Tile> isPassableCheck)
    {
        level.Cast(ray.XY, out var rayCaster);

        while (rayCaster.MoveNext(out var tile))
        {
            if (!level.IsValid(tile) || !isPassableCheck(tile))
            {
                var factor = rayCaster.CurrentRayFactor;
                return new RayCastResult(HitLevel, factor, ray.PointAt(factor), null, rayCaster.LastStep, null);
            }

            var enemies = unitLayer.GetUnitsOnTile(tile);

            var closestHit = default((GameObject unit, float factor, Position3 point, Difference3 normal));
            closestHit.factor = float.PositiveInfinity;

            foreach (var enemy in enemies)
            {
                if (enemy.TryGetSingleComponent<ICollider>(out var collider)
                    && collider.TryHit(ray, out var f, out var point, out var normal) && f < closestHit.factor)
                {
                    closestHit = (enemy, f, point, normal);
                }
            }

            if (closestHit.unit != null)
            {
                return new RayCastResult(
                    HitEnemy, closestHit.factor, closestHit.point,
                    closestHit.unit, rayCaster.LastStep, closestHit.normal);
            }
        }

        return new RayCastResult(HitNothing, 1, ray.PointAtEnd, null, rayCaster.LastStep, null);
    }
}

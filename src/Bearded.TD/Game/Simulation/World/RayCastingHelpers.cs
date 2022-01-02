﻿using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.World.RayCastResultType;

namespace Bearded.TD.Game.Simulation.World;

enum RayCastResultType
{
    HitNothing = 1,
    HitLevel = 2,
    HitEnemy = 4
}

readonly record struct RayCastResult(RayCastResultType Type, float RayFactor, Position2 Point, EnemyUnit? Enemy);

static class RayCastingHelpers
{
    public static IEnumerable<RayCastResult> CastPiercingRayAgainstEnemies(
        this Level level, Ray ray, UnitLayer unitLayer, PassabilityLayer passabilityLayer)
    {
        level.Cast(ray, out var rayCaster);

        while (rayCaster.MoveNext(out var tile))
        {
            if (!level.IsValid(tile) || !passabilityLayer[tile].IsPassable)
            {
                var factor = rayCaster.CurrentRayFactor;
                yield return new RayCastResult(HitLevel, factor, ray.PointAt(factor), null);
                yield break;
            }

            var enemies = unitLayer.GetUnitsOnTile(tile);
            var hits = new List<(EnemyUnit unit, float factor, Position2 point)>();

            foreach (var enemy in enemies)
            {
                if (enemy.CollisionCircle.TryHit(ray, out var f, out var point, out _))
                {
                    hits.Add((enemy, f, point));
                }
            }

            hits.Sort((left, right) => left.factor.CompareTo(right.factor));
            foreach (var (unit, factor, point) in hits)
            {
                yield return new RayCastResult(HitEnemy, factor, point, unit);
            }
        }

        yield return new RayCastResult(HitNothing, 1, ray.PointAtEnd, null);
    }

    public static RayCastResult CastRayAgainstEnemies(
        this Level level, Ray ray, UnitLayer unitLayer, PassabilityLayer passabilityLayer)
    {
        level.Cast(ray, out var rayCaster);

        while (rayCaster.MoveNext(out var tile))
        {
            if (!level.IsValid(tile) || !passabilityLayer[tile].IsPassable)
            {
                var factor = rayCaster.CurrentRayFactor;
                return new RayCastResult(HitLevel, factor, ray.PointAt(factor), null);
            }

            var enemies = unitLayer.GetUnitsOnTile(tile);

            var closestHit = default((EnemyUnit unit, float factor, Position2 point));
            closestHit.factor = float.PositiveInfinity;

            foreach (var enemy in enemies)
            {
                if (enemy.CollisionCircle.TryHit(ray, out var f, out var point, out _) && f < closestHit.factor)
                {
                    closestHit = (enemy, f, point);
                }
            }

            if (closestHit.unit != null)
            {
                return new RayCastResult(
                    HitEnemy, closestHit.factor, closestHit.point, closestHit.unit);
            }
        }

        return new RayCastResult(HitNothing, 1, ray.PointAtEnd, null);
    }
}

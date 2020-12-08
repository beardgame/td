using System.Collections.Generic;
using Bearded.TD.Game.GameState.Navigation;
using Bearded.TD.Game.GameState.Units;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.GameState.World.RayCastResultType;

namespace Bearded.TD.Game.GameState.World
{
    enum RayCastResultType
    {
        HitNothing = 1,
        HitLevel = 2,
        HitEnemy = 4
    }

    struct RayCastResult
    {
        public RayCastResultType Type { get; }
        public float RayFactor { get; }
        public Position2 Point { get; }
        public Maybe<EnemyUnit> Enemy { get; }

        public RayCastResult(RayCastResultType type, float rayFactor, Position2 point, Maybe<EnemyUnit> enemy)
        {
            Type = type;
            RayFactor = rayFactor;
            Point = point;
            Enemy = enemy;
        }

        public void Deconstruct(
            out RayCastResultType type, out float rayFactor, out Position2 point, out Maybe<EnemyUnit> enemy)
        {
            type = Type;
            rayFactor = RayFactor;
            point = Point;
            enemy = Enemy;
        }
    }

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
                    yield return new RayCastResult(HitLevel, factor, ray.PointAt(factor), Maybe.Nothing);
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
                    yield return new RayCastResult(HitEnemy, factor, point, Maybe.Just(unit));
                }
            }

            yield return new RayCastResult(HitNothing, 1, ray.PointAtEnd, Maybe.Nothing);
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
                    return new RayCastResult(HitLevel, factor, ray.PointAt(factor), Maybe.Nothing);
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
                        HitEnemy, closestHit.factor, closestHit.point, Maybe.Just(closestHit.unit));
                }
            }

            return new RayCastResult(HitNothing, 1, ray.PointAtEnd, Maybe.Nothing);
        }
    }
}

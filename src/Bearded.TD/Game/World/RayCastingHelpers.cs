using Bearded.TD.Game.Navigation;
using Bearded.TD.Game.Units;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.World.RayCastResult;

namespace Bearded.TD.Game.World
{
    enum RayCastResult
    {
        HitNothing = 1,
        HitLevel = 2,
        HitEnemy = 4
    }

    static class RayCastingHelpers
    {
        public static (RayCastResult Result, float RayFactor, Position2 Point, EnemyUnit? Enemy)
            CastRayAgainstEnemies(this Level level, Ray ray, UnitLayer unitLayer, PassabilityLayer passabilityLayer)
        {
            level.Cast(ray, out var rayCaster);

            while (rayCaster.MoveNext(out var tile))
            {
                if (!level.IsValid(tile) || !passabilityLayer[tile].IsPassable)
                {
                    var factor = rayCaster.CurrentRayFactor;
                    return (HitLevel, factor, ray.PointAt(factor), null);
                }

                var enemies = unitLayer.GetUnitsOnTile(tile);

                var closestHit = default((EnemyUnit unit, float factor, Position2 point));
                closestHit.factor = float.PositiveInfinity;

                foreach (var enemy in enemies)
                {
                    if (enemy.CollisionCircle.TryHit(ray, out var f, out var point, out _)
                        && f < closestHit.factor)
                    {
                        closestHit = (enemy, f, point);
                    }
                }

                if (closestHit.unit != null)
                {
                    return (HitEnemy, closestHit.factor, closestHit.point, closestHit.unit);
                }
            }

            return (HitNothing, 1, ray.PointAtEnd, null);
        }
    }
}

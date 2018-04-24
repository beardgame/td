using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models.components;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("beamEmitter")]
    sealed class BeamEmitter : WeaponCycleHandler<BeamEmitterParameters>
    {
        private bool drawBeam = false;
        private Position2 endPoint;

        public BeamEmitter(BeamEmitterParameters parameters)
            : base(parameters)
        {
        }

        protected override void UpdateIdle(TimeSpan elapsedTime)
        {
            drawBeam = false;
        }

        protected override void UpdateShooting(TimeSpan elapsedTime)
        {
            emitBeam(elapsedTime);
            
            drawBeam = true;
        }

        private void emitBeam(TimeSpan elapsedTime)
        {
            var ray = new Ray(
                Weapon.Position,
                Weapon.AimDirection * Parameters.Range
            );

            var rayCaster = new LevelRayCaster<TileInfo>();

            foreach (var tile in rayCaster.EnumerateTiles(Game.Level, ray))
            {
                if (!tile.IsValid || !tile.Info.IsPassableFor(TileInfo.PassabilityLayer.Projectile))
                {
                    endPoint = rayCaster.CurrentPoint(ray);
                    return;
                }

                var enemies = tile.Info.Enemies;

                (EnemyUnit Unit, float Factor, Position2 point) closestHit =
                    (null, float.PositiveInfinity, new Position2());

                foreach (var enemy in enemies)
                {
                    if (enemy.CollisionCircle.TryHit(ray, out var f, out var point, out _)
                        && f < closestHit.Factor)
                    {
                        closestHit = (enemy, f, point);
                    }
                }

                if (closestHit.Unit != null)
                {
                    closestHit.Unit.Damage(
                        StaticRandom.Discretise(
                            (float) (Parameters.DamagePerSecond * elapsedTime.NumericValue)
                        ),
                        Weapon.Owner as Building
                    );
                    endPoint = closestHit.point;
                    return;
                }
            }

            endPoint = ray.Start + ray.Direction;
        }

        public override void Draw(GeometryManager geometries)
        {
            if (!drawBeam)
                return;

            var geo = geometries.ConsoleBackground;
            geo.Color = Parameters.Color.WithAlpha() * StaticRandom.Float(0.5f, 0.8f);
            geo.LineWidth = 0.1f;

            geo.DrawLine(Weapon.Position.NumericValue, endPoint.NumericValue);
        }
    }
}

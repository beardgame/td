using System;
using amulware.Graphics;
using System.Collections.Generic;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Damage;
using Bearded.TD.Game.Navigation;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Rendering;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Components.Weapons
{
    [Component("beamEmitter")]
    sealed class BeamEmitter : WeaponCycleHandler<IBeamEmitterParameters>
    {
        private const double minDamagePerSecond = .05;

        private bool drawBeam;
        private readonly List<(Position2 start, Position2 end, float damageFactor)> beamSegments =
            new List<(Position2 start, Position2 end, float damageFactor)>();

        public BeamEmitter(IBeamEmitterParameters parameters)
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
                Weapon.Position.XY(),
                Weapon.CurrentDirection * Parameters.Range
            );

            var results = Parameters.PiercingFactor > minDamagePerSecond
                ? Game.Level.CastPiercingRayAgainstEnemies(
                    ray, Game.UnitLayer, Game.PassabilityManager.GetLayer(Passability.Projectile))
                : Game.Level.CastRayAgainstEnemies(
                    ray, Game.UnitLayer, Game.PassabilityManager.GetLayer(Passability.Projectile)).Yield();

            beamSegments.Clear();
            var lastEnd = Weapon.Position.XY();
            var damageFactor = 1.0f;

            foreach (var (type, _, point, enemy) in results)
            {
                switch (type)
                {
                    case RayCastResultType.HitNothing:
                    case RayCastResultType.HitLevel:
                        beamSegments.Add((lastEnd, point, damageFactor));
                        break;
                    case RayCastResultType.HitEnemy:
                        if (damageFactor * Parameters.DamagePerSecond > minDamagePerSecond)
                        {
                            var damagePerSecond = damageFactor * Parameters.DamagePerSecond;
                            enemy.Match(
                                e => damageEnemy(e, damagePerSecond, elapsedTime),
                                () => throw new InvalidOperationException());

                            beamSegments.Add((lastEnd, point, damageFactor));
                            damageFactor *= Parameters.PiercingFactor;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void damageEnemy(EnemyUnit enemy, double damagePerSecond, TimeSpan elapsedTime)
        {
            enemy.Damage(new DamageInfo(
                StaticRandom.Discretise((float) (damagePerSecond * elapsedTime.NumericValue)),
                DamageType.Energy,
                Weapon.Owner as Building
            ));
        }

        public override void Draw(GeometryManager geometries)
        {
            if (!drawBeam)
            {
                return;
            }

            var shapeDrawer = geometries.ConsoleBackground;
            var baseAlpha = StaticRandom.Float(0.5f, 0.8f);

            foreach (var (start, end, factor) in beamSegments)
            {
                shapeDrawer.DrawLine(
                    start.WithZ(Weapon.Position.Z).NumericValue,
                    end.WithZ(Weapon.Position.Z).NumericValue,
                    Parameters.Width.NumericValue * PixelSize * 0.5f,
                    Parameters.Color.WithAlpha() * baseAlpha * factor);

                shapeDrawer.DrawLine(
                    start.WithZ(Weapon.Position.Z).NumericValue,
                    end.WithZ(Weapon.Position.Z).NumericValue,
                    Parameters.CoreWidth.NumericValue * PixelSize * 0.5f,
                    Color.White.WithAlpha() * baseAlpha * factor);
            }
        }
    }
}

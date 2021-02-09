using System;
using System.Collections.Generic;
using amulware.Graphics;
using amulware.Graphics.Shapes;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Rendering;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components.Weapons
{
    [Component("beamEmitter")]
    sealed class BeamEmitter : WeaponCycleHandler<IBeamEmitterParameters>
    {
        private static readonly TimeSpan damageTimeSpan = 0.1.S();
        private const double minDamagePerSecond = .05;

        private Instant? lastDamageTime;
        private bool drawBeam;
        private readonly List<(Position2 start, Position2 end, float damageFactor)> beamSegments = new();

        public BeamEmitter(IBeamEmitterParameters parameters)
            : base(parameters)
        {
        }

        protected override void UpdateIdle(TimeSpan elapsedTime)
        {
            lastDamageTime = null;
            drawBeam = false;
        }

        protected override void UpdateShooting(TimeSpan elapsedTime)
        {
            lastDamageTime ??= Game.Time;

            emitBeam();

            drawBeam = true;
        }

        private void emitBeam()
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
                                e => tryDamageEnemy(e, damagePerSecond),
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

        private void tryDamageEnemy(IMortal enemy, double damagePerSecond)
        {
            if (lastDamageTime == null)
            {
                throw new InvalidOperationException();
            }

            var timeSinceLastDamage = Game.Time - lastDamageTime.Value;
            if (timeSinceLastDamage < damageTimeSpan)
            {
                return;
            }

            var result = enemy.Damage(new DamageInfo(
                StaticRandom.Discretise((float) (damagePerSecond * timeSinceLastDamage.NumericValue)).HitPoints(),
                DamageType.Energy,
                Weapon.Owner as Building
            ));
            lastDamageTime = Game.Time;
            Events.Send(new CausedDamage(enemy, result));
        }

        public override void Draw(CoreDrawers drawers)
        {
            if (!drawBeam)
            {
                return;
            }

            var shapeDrawer = drawers.ConsoleBackground;
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

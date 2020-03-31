﻿using System;
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
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Components.Weapons
{
    [Component("beamEmitter")]
    sealed class BeamEmitter : WeaponCycleHandler<IBeamEmitterParameters>
    {
        private bool drawBeam;
        private Position2 endPoint;

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

            var (result, _, point, enemy) = Game.Level.CastRayAgainstEnemies(
                ray, Game.UnitLayer, Game.PassabilityManager.GetLayer(Passability.Projectile));

            endPoint = point;

            switch (result)
            {
                case RayCastResultType.HitNothing:
                case RayCastResultType.HitLevel:
                    break;
                case RayCastResultType.HitEnemy:
                    enemy.Match(e => damageEnemy(e, elapsedTime), () => throw new InvalidOperationException());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void damageEnemy(EnemyUnit enemy, TimeSpan elapsedTime)
        {
            enemy.Damage(new DamageInfo(
                StaticRandom.Discretise(
                    (float) (Parameters.DamagePerSecond * elapsedTime.NumericValue)
                ),
                DamageType.Energy,
                Weapon.Owner as Building
            ));
        }

        public override void Draw(GeometryManager geometries)
        {
            if (!drawBeam)
                return;

            var geo = geometries.ConsoleBackground;
            geo.Color = Parameters.Color.WithAlpha() * StaticRandom.Float(0.5f, 0.8f);
            geo.LineWidth = Constants.Rendering.PixelSize;

            geo.DrawLine(Weapon.Position.NumericValue, endPoint.WithZ(Weapon.Position.Z).NumericValue);
        }
    }
}

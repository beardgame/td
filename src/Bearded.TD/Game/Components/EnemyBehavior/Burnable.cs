using System;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Damage;
using Bearded.TD.Game.Elements;
using Bearded.TD.Game.Units;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities;
using static Bearded.TD.Constants.Game.Elements;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Components.EnemyBehavior
{
    [Component("burnable")]
    sealed class Burnable<T> : Component<T, IBurnableParameters> where T : EnemyUnit, IMortal, IPositionable
    {
        private Building lastFireHitBuilding;
        private Building damageSourceBuilding;

        private Combustable combustable;
        private double damagePerFuel;

        public Burnable(IBurnableParameters parameters) : base(parameters) {}

        protected override void Initialise()
        {
            combustable = new Combustable(
                Parameters.FuelAmount,
                Parameters.FlashPointThreshold,
                Parameters.BurnSpeed ?? new EnergyConsumptionRate(1));
            damagePerFuel = Parameters.DamagePerFuel ?? 1;
            Owner.Damaged += onDamaged;
        }

        private void onDamaged(DamageInfo damage)
        {
            switch (damage.Type)
            {
                case DamageType.Energy:
                    combustable.HitWithFire(damage.Amount * EnergyPerEnergyDamage);
                    break;
                case DamageType.Fire:
                    combustable.HitWithFire(damage.Amount * EnergyPerFireDamage);
                    break;
            }

            damage.Source.Match(building => lastFireHitBuilding = building);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            combustable.Update(elapsedTime);

            if (!combustable.IsOnFire) return;

            if (damageSourceBuilding == null)
            {
                damageSourceBuilding = lastFireHitBuilding;
            }

            Owner.Damage(
                new DamageInfo(
                    StaticRandom.Discretise(
                        (float) (elapsedTime.NumericValue * damagePerFuel * combustable.BurningSpeed.NumericValue)),
                    DamageType.Fire,
                    damageSourceBuilding));
        }

        public override void Draw(GeometryManager geometries)
        {
            if (!combustable.IsOnFire) return;

            var geo = geometries.ConsoleBackground;
            geo.Color = Color.OrangeRed * .8f;
            geo.LineWidth = .1f;
            geo.DrawCircle(Owner.Position.NumericValue, 1.5f, false);
        }
    }
}

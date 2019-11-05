using amulware.Graphics;
using Bearded.TD.Content.Models;
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

        private void onDamaged(int damage, DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Energy:
                    combustable.HitWithFire(damage * EnergyPerEnergyDamage);
                    break;
                case DamageType.Fire:
                    combustable.HitWithFire(damage * EnergyPerFireDamage);
                    break;
            }
        }

        public override void Update(TimeSpan elapsedTime)
        {
            combustable.Update(elapsedTime);

            if (combustable.IsOnFire)
            {
                Owner.Damage(
                    StaticRandom.Discretise(
                        (float) (elapsedTime.NumericValue * damagePerFuel * combustable.BurningSpeed.NumericValue)),
                    DamageType.Fire,
                    null);
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            if (!combustable.IsOnFire) return;

            var primitives = geometries.Primitives;
            primitives.Color = Color.OrangeRed;
            primitives.DrawCircle(Owner.Position.NumericValue, Constants.Game.World.HexagonSide, false);
        }
    }
}

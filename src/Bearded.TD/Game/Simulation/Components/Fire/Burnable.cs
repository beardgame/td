using System;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities;
using static Bearded.TD.Constants.Game.Elements;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components.Fire
{
    [Component("burnable")]
    sealed class Burnable<T> : Component<T, IBurnableParameters>, IListener<TookDamage>, IListener<Spark>
        where T : GameObject, IPositionable
    {
        private IDamageOwner lastFireHitOwner;
        private IDamageOwner damageSource;

        private Combustable combustable;
        private double damagePerFuel;

        private float fireRenderStrengthGoal = 1;
        private float fireRenderStrength;
        private bool dealingDamageToOwner;

        public Burnable(IBurnableParameters parameters) : base(parameters) {}

        protected override void Initialise()
        {
            combustable = new Combustable(
                Parameters.FuelAmount,
                Parameters.FlashPointThreshold,
                Parameters.BurnSpeed ?? new EnergyConsumptionRate(1),
                Parameters.StartsOnFire);

            combustable.Extinguished += onExtinguished;

            damagePerFuel = Parameters.DamagePerFuel ?? 1;
            Events.Subscribe<TookDamage>(this);
            Events.Subscribe<Spark>(this);
        }

        private void onExtinguished()
        {
            Events.Send(new FireExtinguished());
        }

        public void HandleEvent(TookDamage @event)
        {
            if (dealingDamageToOwner)
                return;

            onDamaged(@event.Damage);
        }

        public void HandleEvent(Spark @event)
        {
            combustable.Spark();
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

            lastFireHitOwner = damage.Source;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            var (volume, _) = Owner.Game.FluidLayer.Water[Level.GetTile(Owner.Position.XY())];

            if (volume > Volume.Zero)
            {
                combustable.HitWithWater(elapsedTime * EnergyPerSecondInWater);
            }

            combustable.Update(elapsedTime);

            if (!combustable.IsOnFire) return;

            if (damageSource == null)
            {
                damageSource = lastFireHitOwner;
            }

            var damage = new DamageInfo(
                StaticRandom.Discretise((float) (elapsedTime.NumericValue * damagePerFuel * combustable.BurningSpeed.NumericValue)),
                DamageType.Fire,
                damageSource);

            dealingDamageToOwner = true;
            Events.Send(new TakeDamage(damage));
            dealingDamageToOwner = false;

            if (StaticRandom.Bool(elapsedTime.NumericValue * 10))
                fireRenderStrengthGoal = StaticRandom.Float(0.5f, 1);

            fireRenderStrength += (fireRenderStrengthGoal - fireRenderStrength) * (1 - (float)Math.Pow(0.001, elapsedTime.NumericValue));
        }

        public override void Draw(CoreDrawers geometries)
        {
            if (!combustable.IsOnFire) return;

            geometries.PointLight.Draw(
                Owner.Position.NumericValue,
                1.5f * fireRenderStrength,
                Color.OrangeRed * fireRenderStrength
            );
        }
    }
}

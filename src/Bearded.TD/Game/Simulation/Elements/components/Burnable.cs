using System;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities;
using static Bearded.TD.Constants.Game.Elements;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements
{
    [Component("burnable")]
    sealed class Burnable<T> : Component<T, IBurnableParameters>, IListener<TakeDamage>, IListener<Spark>
        where T : IComponentOwner, IGameObject, IPositionable
    {
        private IDamageSource? lastFireHitOwner;
        private IDamageSource? damageSource;

        private Combustable combustable = null!;
        private double damagePerFuel;

        private float fireRenderStrengthGoal = 1;
        private float fireRenderStrength;
        private bool dealingDamageToOwner;

        public Burnable(IBurnableParameters parameters) : base(parameters) {}

        protected override void OnAdded()
        {
            combustable = new Combustable(
                Parameters.FuelAmount,
                Parameters.FlashPointThreshold,
                Parameters.BurnSpeed ?? new EnergyConsumptionRate(1),
                Parameters.StartsOnFire);

            combustable.Extinguished += onExtinguished;

            damagePerFuel = Parameters.DamagePerFuel ?? 1;
            Events.Subscribe<TakeDamage>(this);
            Events.Subscribe<Spark>(this);
        }

        public override void OnRemoved()
        {
            Events.Unsubscribe<TakeDamage>(this);
            Events.Unsubscribe<Spark>(this);
        }

        private void onExtinguished()
        {
            Events.Send(new FireExtinguished());
        }

        public void HandleEvent(TakeDamage @event)
        {
            if (dealingDamageToOwner)
                return;

            onDamaged(@event.Damage, @event.Source);
        }

        public void HandleEvent(Spark @event)
        {
            combustable.Spark();
        }

        private void onDamaged(DamageResult result, IDamageSource? source)
        {
            switch (result.Damage.Type)
            {
                case DamageType.Energy:
                    combustable.HitWithFire(result.Damage.Amount.NumericValue * EnergyPerEnergyDamage);
                    break;
                case DamageType.Fire:
                    combustable.HitWithFire(result.Damage.Amount.NumericValue * EnergyPerFireDamage);
                    break;
            }

            lastFireHitOwner = source ?? lastFireHitOwner;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            var (volume, _) = Owner.Game.FluidLayer.Water[Level.GetTile(Owner.Position.XY())];

            if (volume > Volume.Zero)
            {
                combustable.HitWithWater(elapsedTime * EnergyPerSecondInWater);
            }

            combustable.Update(elapsedTime);

            if (!combustable.IsOnFire)
            {
                return;
            }

            damageSource ??= lastFireHitOwner;

            dealingDamageToOwner = true;
            var damage = new DamageInfo(
                StaticRandom
                    .Discretise((float)(elapsedTime.NumericValue * damagePerFuel *
                        combustable.BurningSpeed.NumericValue)).HitPoints(),
                DamageType.Fire);
            DamageExecutor.FromDamageSource(damageSource).TryDoDamage(Owner, damage);
            dealingDamageToOwner = false;

            if (StaticRandom.Bool(elapsedTime.NumericValue * 10))
                fireRenderStrengthGoal = StaticRandom.Float(0.5f, 1);

            fireRenderStrength += (fireRenderStrengthGoal - fireRenderStrength) * (1 - (float)Math.Pow(0.001, elapsedTime.NumericValue));
        }

        public override void Draw(CoreDrawers drawers)
        {
            if (!combustable.IsOnFire) return;

            drawers.PointLight.Draw(
                Owner.Position.NumericValue,
                1.5f * fireRenderStrength,
                Color.OrangeRed * fireRenderStrength
            );
        }
    }
}

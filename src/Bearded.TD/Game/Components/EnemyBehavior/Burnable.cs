using System;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components.Events;
using Bearded.TD.Game.Damage;
using Bearded.TD.Game.Elements;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities;
using static Bearded.TD.Constants.Game.Elements;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Components.EnemyBehavior
{
    [Component("burnable")]
    sealed class Burnable<T> : Component<T, IBurnableParameters>, IListener<TookDamage>, IListener<Spark>
        where T : GameObject, IEventManager, IPositionable
    {
        private Building lastFireHitBuilding;
        private Building damageSourceBuilding;

        private Combustable combustable;
        private double damagePerFuel;

        private float fireRenderStrengthGoal = 1;
        private float fireRenderStrength = 0;
        private bool dealingDamageToOwner;

        public Burnable(IBurnableParameters parameters) : base(parameters) {}

        protected override void Initialise()
        {
            combustable = new Combustable(
                Parameters.FuelAmount,
                Parameters.FlashPointThreshold,
                Parameters.BurnSpeed ?? new EnergyConsumptionRate(1));

            combustable.Extinguished += onExtinguished;

            damagePerFuel = Parameters.DamagePerFuel ?? 1;
            Owner.Events.Subscribe<TookDamage>(this);
            Owner.Events.Subscribe<Spark>(this);
        }

        private void onExtinguished()
        {
            Owner.Events.Send(new FireExtinguished());
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

            damage.Source.Match(building => lastFireHitBuilding = building);
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

            if (damageSourceBuilding == null)
            {
                damageSourceBuilding = lastFireHitBuilding;
            }

            var damage = new DamageInfo(
                StaticRandom.Discretise((float) (elapsedTime.NumericValue * damagePerFuel * combustable.BurningSpeed.NumericValue)),
                DamageType.Fire,
                damageSourceBuilding);

            dealingDamageToOwner = true;
            Owner.Events.Send(new TakeDamage(damage));
            dealingDamageToOwner = false;

            if (StaticRandom.Bool(elapsedTime.NumericValue * 10))
                fireRenderStrengthGoal = StaticRandom.Float(0.5f, 1);

            fireRenderStrength += (fireRenderStrengthGoal - fireRenderStrength) * (1 - (float)Math.Pow(0.001, elapsedTime.NumericValue));
        }

        public override void Draw(GeometryManager geometries)
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

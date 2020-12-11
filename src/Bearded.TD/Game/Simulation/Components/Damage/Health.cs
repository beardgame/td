using System;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Meta;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components.Damage
{
    [Component("health")]
    class Health<T> : Component<T, IHealthComponentParameter>, ISyncable, IListener<TakeDamage> where T : IMortal
    {
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public double HealthPercentage => (double) CurrentHealth / MaxHealth;

        public Health(IHealthComponentParameter parameters) : base(parameters)
        {
            CurrentHealth = parameters.InitialHealth ?? parameters.MaxHealth;
            MaxHealth = parameters.MaxHealth;
        }

        protected override void Initialise()
        {
            Owner.Healed += onHealed;
            Events.Subscribe(this);
        }

        public void HandleEvent(TakeDamage @event)
        {
            onDamaged(@event.Damage);
        }

        private void onDamaged(DamageInfo damage)
        {
            if (damage.Amount > 0 && UserSettings.Instance.Debug.InvulnerableBuildings && Owner is Building)
                return;

            changeHealth(-damage.Amount);

            Events.Send(new TookDamage(damage));
        }

        private void onHealed(int health)
        {
            changeHealth(health);
        }

        private void changeHealth(int healthChange)
        {
            CurrentHealth = (CurrentHealth + healthChange).Clamped(0, MaxHealth);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (CurrentHealth <= 0)
            {
                Owner.OnDeath();
            }
        }

        public override void Draw(GeometryManager geometries) { }

        public override void ApplyUpgradeEffect(IUpgradeEffect effect)
        {
            base.ApplyUpgradeEffect(effect);

            if (Parameters.MaxHealth != MaxHealth)
            {
                applyNewMaxHealth();
            }
        }

        private void applyNewMaxHealth()
        {
            if (Parameters.MaxHealth > MaxHealth)
            {
                CurrentHealth += Parameters.MaxHealth - MaxHealth;
                MaxHealth = Parameters.MaxHealth;
            }
            else
            {
                MaxHealth = Parameters.MaxHealth;
                CurrentHealth = Math.Min(CurrentHealth, MaxHealth);
            }
        }

        public IStateToSync GetCurrentStateToSync() => new HealthSynchronizedState(this);

        private class HealthSynchronizedState : IStateToSync
        {
            private readonly Health<T> source;
            private int currentHealth;

            public HealthSynchronizedState(Health<T> source)
            {
                this.source = source;
                currentHealth = source.CurrentHealth;
            }

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref currentHealth);
            }

            public void Apply()
            {
                source.CurrentHealth = currentHealth;
            }
        }
    }
}

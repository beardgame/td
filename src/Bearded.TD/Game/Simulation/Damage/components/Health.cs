using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Meta;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Damage
{
    interface IHealth
    {
        HitPoints CurrentHealth { get; }
        HitPoints MaxHealth { get; }
        double HealthPercentage => CurrentHealth / MaxHealth;
    }

    [Component("health")]
    sealed class Health<T> :
        Component<T, IHealthComponentParameter>,
        IHealth,
        ISyncable,
        IListener<HealDamage>,
        IPreviewListener<TakeDamage>
        where T : IMortal
    {
        public HitPoints CurrentHealth { get; private set; }
        public HitPoints MaxHealth { get; private set; }

        public Health(IHealthComponentParameter parameters) : base(parameters)
        {
            CurrentHealth = parameters.InitialHealth ?? parameters.MaxHealth;
            MaxHealth = parameters.MaxHealth;
        }

        protected override void Initialize()
        {
            Events.Subscribe<HealDamage>(this);
            Events.Subscribe<TakeDamage>(this);
            Events.Send(new ReportAdded(new HealthReport(this)));
        }

        public void HandleEvent(HealDamage @event)
        {
            onHealed(@event.Amount);
        }

        public void PreviewEvent(ref TakeDamage @event)
        {
            var oldHealth = CurrentHealth;
            onDamaged(@event.Damage);
            var damageDone = oldHealth - CurrentHealth;
            @event = @event.DamageAdded(damageDone);
        }

        private void onDamaged(DamageInfo damage)
        {
            if (damage.Amount > HitPoints.Zero && UserSettings.Instance.Debug.InvulnerableBuildings && Owner is Building)
            {
                return;
            }

            changeHealth(-damage.Amount);
        }

        private void onHealed(HitPoints health)
        {
            changeHealth(health);
        }

        private void changeHealth(HitPoints healthChange)
        {
            CurrentHealth = DiscreteSpaceTime1Math.Clamp(CurrentHealth + healthChange, HitPoints.Zero, MaxHealth);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (CurrentHealth <= HitPoints.Zero)
            {
                Owner.OnDeath();
            }
        }

        public override void Draw(CoreDrawers drawers) { }

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
                CurrentHealth = DiscreteSpaceTime1Math.Min(CurrentHealth, MaxHealth);
            }
        }

        public IStateToSync GetCurrentStateToSync() => new HealthSynchronizedState(this);

        private sealed class HealthSynchronizedState : IStateToSync
        {
            private readonly Health<T> source;
            private int currentHealth;

            public HealthSynchronizedState(Health<T> source)
            {
                this.source = source;
                currentHealth = source.CurrentHealth.NumericValue;
            }

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref currentHealth);
            }

            public void Apply()
            {
                source.CurrentHealth = new HitPoints(currentHealth);
            }
        }

        private sealed class HealthReport : IHealthReport
        {
            public ReportType Type => ReportType.EntityProperties;

            public HitPoints CurrentHealth => source.CurrentHealth;
            public HitPoints MaxHealth => source.MaxHealth;

            private readonly Health<T> source;

            public HealthReport(Health<T> source)
            {
                this.source = source;
            }
        }
    }
}

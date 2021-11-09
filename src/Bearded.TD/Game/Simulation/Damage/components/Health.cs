using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;
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
        IPreviewListener<PreviewHealDamage>,
        IListener<HealDamage>,
        IPreviewListener<PreviewTakeDamage>,
        IListener<TakeDamage>
    {
        public HitPoints CurrentHealth { get; private set; }
        public HitPoints MaxHealth { get; private set; }

        public Health(IHealthComponentParameter parameters) : base(parameters)
        {
            CurrentHealth = parameters.InitialHealth ?? parameters.MaxHealth;
            MaxHealth = parameters.MaxHealth;
        }

        protected override void OnAdded()
        {
            Events.Subscribe<PreviewHealDamage>(this);
            Events.Subscribe<HealDamage>(this);
            Events.Subscribe<PreviewTakeDamage>(this);
            Events.Subscribe<TakeDamage>(this);
            ReportAggregator.Register(Events, new HealthReport(this));
        }

        public override void OnRemoved()
        {
            State.IsInvalid("Can never remove health components.");
        }

        public void PreviewEvent(ref PreviewHealDamage @event)
        {
            @event = @event.CappedAt(MaxHealth - CurrentHealth);
        }

        public void HandleEvent(HealDamage @event)
        {
            onHealed(@event.Heal.Heal);
        }

        public void PreviewEvent(ref PreviewTakeDamage @event)
        {
            @event = @event.CappedAt(CurrentHealth);
        }

        public void HandleEvent(TakeDamage @event)
        {
            onDamaged(@event.Damage.Damage);
        }

        private void onHealed(HealInfo heal)
        {
            changeHealth(heal.Amount);
        }

        private void onDamaged(DamageInfo damage)
        {
            changeHealth(-damage.Amount);
        }

        private void changeHealth(HitPoints healthChange)
        {
            CurrentHealth = DiscreteSpaceTime1Math.Clamp(CurrentHealth + healthChange, HitPoints.Zero, MaxHealth);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (CurrentHealth <= HitPoints.Zero)
            {
                Events.Send(new EnactDeath());
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

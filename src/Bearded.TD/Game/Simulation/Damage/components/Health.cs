using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Damage;

interface IHealth
{
    HitPoints CurrentHealth { get; }
    HitPoints MaxHealth { get; }
    double HealthPercentage => CurrentHealth / MaxHealth;
}

[Component("health")]
sealed class Health :
    Component<Health.IParameters>,
    IHealth,
    IDamageReceiver,
    ISyncable,
    IPreviewListener<PreviewHealDamage>,
    IListener<HealDamage>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1, Type = AttributeType.Health)]
        HitPoints MaxHealth { get; }

        HitPoints? InitialHealth { get; }
    }

    public HitPoints CurrentHealth { get; private set; }
    public HitPoints MaxHealth { get; private set; }
    public DamageShell Shell => DamageShell.Health;

    public Health(IParameters parameters) : base(parameters)
    {
        CurrentHealth = parameters.InitialHealth ?? parameters.MaxHealth;
        MaxHealth = parameters.MaxHealth;
    }

    protected override void OnAdded()
    {
        Events.Subscribe<PreviewHealDamage>(this);
        Events.Subscribe<HealDamage>(this);
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

    private void onHealed(HealInfo heal)
    {
        changeHealth(heal.Amount, out _);
    }

    public IntermediateDamageResult ApplyDamage(TypedDamage damage, IDamageSource? source)
    {
        // No health, so object is already dead.
        if (CurrentHealth <= HitPoints.Zero)
        {
            return IntermediateDamageResult.PassThrough(damage);
        }

        var e = new PreviewTakeDamage(damage);
        Events.Preview(ref e);
        var resistance = e.Resistance ?? Resistance.Zero;
        var resistedDamage = resistance.ApplyToDamage(damage);

        if (resistedDamage.Amount <= HitPoints.Zero)
        {
            return IntermediateDamageResult.Blocked(damage);
        }

        var cappedDamage =
            resistedDamage.WithAdjustedAmount(SpaceTime1MathF.Min(resistedDamage.Amount, CurrentHealth));
        changeHealth(-cappedDamage.Amount, out var damageDoneDiscrete);

        var result = new IntermediateDamageResult(cappedDamage, TypedDamage.Zero(damage.Type), damageDoneDiscrete);

        Events.Send(new TookDamage(result.AsFinal(), source));

        return result;
    }

    private void changeHealth(HitPoints healthChange, out HitPoints damageDoneDiscrete)
    {
        var oldHealthDiscrete = CurrentHealth.Discrete();
        CurrentHealth = SpaceTime1MathF.Clamp(CurrentHealth + healthChange, HitPoints.Zero, MaxHealth);
        var newHealthDiscrete = CurrentHealth.Discrete();
        // This expression may look inverted, but that's because we want the difference as a positive number.
        damageDoneDiscrete = oldHealthDiscrete - newHealthDiscrete;
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Parameters.MaxHealth != MaxHealth)
        {
            applyNewMaxHealth();
        }
        if (CurrentHealth <= HitPoints.Zero)
        {
            Events.Send(new EnactDeath());
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
            CurrentHealth = SpaceTime1MathF.Min(CurrentHealth, MaxHealth);
        }
    }

    public IStateToSync GetCurrentStateToSync() => new HealthSynchronizedState(this);

    private sealed class HealthSynchronizedState : IStateToSync
    {
        private readonly Health source;
        private float currentHealth;

        public HealthSynchronizedState(Health source)
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

        private readonly Health source;

        public HealthReport(Health source)
        {
            this.source = source;
        }
    }
}

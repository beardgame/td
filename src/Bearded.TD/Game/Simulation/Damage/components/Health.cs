using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
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
    HitPointsPool<Health.IParameters>,
    IHealth,
    IPreviewListener<PreviewHealDamage>,
    IListener<HealDamage>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1, Type = AttributeType.Health)]
        HitPoints MaxHealth { get; }

        HitPoints? InitialHealth { get; }
    }

    public HitPoints CurrentHealth => CurrentHitPoints;
    public HitPoints MaxHealth => MaxHitPoints;

    protected override HitPoints TargetMaxHitPoints => Parameters.MaxHealth;
    public override DamageShell Shell => DamageShell.Health;

    public Health(IParameters parameters) : base(parameters, parameters.MaxHealth)
    {
        if (Parameters.InitialHealth is { } initialHealth)
        {
            OverrideCurrentHitPoints(initialHealth);
        }
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
        RestoreHitPoints(@event.Heal.Heal.Amount);
    }

    protected override TypedDamage ModifyDamage(TypedDamage damage)
    {
        var e = new PreviewTakeDamage(damage);
        Events.Preview(ref e);
        var resistance = e.Resistance ?? Resistance.Zero;
        return resistance.ApplyToDamage(damage);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        base.Update(elapsedTime);
        if (CurrentHealth <= HitPoints.Zero)
        {
            Events.Send(new EnactDeath());
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

using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("targetingModes")]
sealed class TargetingModeProperty : Component<TargetingModeProperty.IParameters>, IProperty<ITargetingMode>, ITargetingModeSetter
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        ImmutableArray<ITargetingMode>? AllowedTargetingModes { get; }
        ITargetingMode? DefaultTargetingMode { get; }
    }

    public ITargetingMode Value { get; private set; } = TargetingMode.Default;

    public ImmutableArray<ITargetingMode> AllowedTargetingModes =>
        Parameters.AllowedTargetingModes ?? TargetingMode.All;

    public TargetingModeProperty(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ReportAggregator.Register(Events, new TargetingReport(this));
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void SetTargetingMode(ITargetingMode newMode)
    {
        Value = newMode;
    }

    private sealed class TargetingReport : ITargetingReport
    {
        private readonly TargetingModeProperty subject;

        public ReportType Type => ReportType.EntityMode;

        public GameObject Object => subject.Owner;
        public ImmutableArray<ITargetingMode> AvailableTargetingModes => subject.AllowedTargetingModes;
        public ITargetingMode TargetingMode => subject.Value;

        public TargetingReport(TargetingModeProperty subject)
        {
            this.subject = subject;
        }
    }
}

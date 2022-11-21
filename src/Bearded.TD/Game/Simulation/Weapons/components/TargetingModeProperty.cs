using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("targetingModes")]
sealed class TargetingModeProperty : Component<TargetingModeProperty.IParameters>, IProperty<ITargetingMode>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        ImmutableArray<ITargetingMode>? AllowedTargetingModes { get; }
        ITargetingMode? DefaultTargetingMode { get; }
    }

    public ITargetingMode Value { get; private set; } = TargetingMode.Default;

    public TargetingModeProperty(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ReportAggregator.Register(
            Events, new TargetingReport(this, Parameters.AllowedTargetingModes ?? TargetingMode.All));
    }

    public override void Update(TimeSpan elapsedTime) { }

    private sealed class TargetingReport : ITargetingReport
    {
        private readonly TargetingModeProperty subject;

        public ReportType Type => ReportType.EntityMode;

        public ImmutableArray<ITargetingMode> AvailableTargetingModes { get; }

        public ITargetingMode TargetingMode
        {
            get => subject.Value;
            set => subject.Value = value;
        }

        public TargetingReport(TargetingModeProperty subject, ImmutableArray<ITargetingMode> availableTargetingModes)
        {
            this.subject = subject;
            AvailableTargetingModes = availableTargetingModes;
        }
    }
}

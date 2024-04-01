using System;
using System.Collections.Immutable;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Shared.TechEffects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("targetingModes")]
sealed class TargetingModeProperty : Component<TargetingModeProperty.IParameters>, IProperty<ITargetingMode>, ITargetingModeSetter
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        ImmutableArray<ITargetingMode>? AllowedTargetingModes { get; }
        ITargetingMode? DefaultTargetingMode { get; }
    }

    private IStatusReceipt? statusReceipt;

    public ITargetingMode Value { get; private set; } = TargetingMode.Default;

    public ImmutableArray<ITargetingMode> AllowedTargetingModes =>
        Parameters.AllowedTargetingModes ?? TargetingMode.All;

    public TargetingModeProperty(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ReportAggregator.Register(Events, new TargetingReport(this));
    }

    public override void Activate()
    {
        base.Activate();
        if (Parameters.DefaultTargetingMode is { } mode)
        {
            if (AllowedTargetingModes.Contains(mode))
            {
                Value = mode;
            }
            else
            {
                Owner.Game.Meta.Logger.Debug?.Log($"Default targeting mode {mode} not actually allowed.");
            }
        }

        if (Owner.TryGetSingleComponent<IStatusTracker>(out var statusTracker))
        {
            statusReceipt = statusTracker.AddStatus(
                new StatusSpec(
                    StatusType.Neutral,
                    new InteractionSpec(this),
                    new EmptyStatusDrawer()),
                StatusAppearance.IconOnly(Value.Icon),
                null);
        }
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void SetTargetingMode(ITargetingMode newMode)
    {
        Value = newMode;
        statusReceipt?.UpdateAppearance(StatusAppearance.IconOnly(Value.Icon));
        Events.Send(new TargetingModeChanged());
    }

    [Obsolete]
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

    private sealed class InteractionSpec(TargetingModeProperty subject) : IStatusInteractionSpec
    {
        public void Interact(GameRequestDispatcher requestDispatcher)
        {
            var currentIndex = subject.AllowedTargetingModes.IndexOf(subject.Value);
            var newIndex = (currentIndex + 1) % subject.AllowedTargetingModes.Length;
            var newMode = subject.AllowedTargetingModes[newIndex];
            requestDispatcher.Request(Buildings.SetTargetingMode.Request, subject.Owner, newMode);
        }
    }
}

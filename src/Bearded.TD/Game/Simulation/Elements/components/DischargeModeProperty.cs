using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("dischargeMode")]
sealed class DischargeModeProperty : Component, IProperty<CapacitorDischargeMode>, IDischargeModeSetter
{
    public CapacitorDischargeMode Value { get; private set; } = CapacitorDischargeMode.MinimumCharge;

    protected override void OnAdded()
    {
        ReportAggregator.Register(Events, new DischargeModeReport(this));
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void SetDischargeMode(CapacitorDischargeMode newMode)
    {
        Value = newMode;
    }

    private sealed class DischargeModeReport : IDischargeModeReport
    {
        private readonly DischargeModeProperty subject;

        public ReportType Type => ReportType.EntityMode;
        public GameObject Object => subject.Owner;
        public CapacitorDischargeMode DischargeMode => subject.Value;

        public DischargeModeReport(DischargeModeProperty subject)
        {
            this.subject = subject;
        }
    }
}

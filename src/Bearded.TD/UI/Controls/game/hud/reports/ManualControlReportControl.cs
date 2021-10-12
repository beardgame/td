using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.UI.Factories;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.UI.Controls
{
    sealed class ManualControlReportControl : ReportControl
    {
        private readonly IManualControlReport report;

        private bool active;

        public override double Height { get; }

        public ManualControlReportControl(IManualControlReport report)
        {
            this.report = report;

            var column = this.BuildFixedColumn();
            column.AddButton(b => b.WithLabel("Assume Direct Control").WithOnClick(startControl));
            Height = column.Height;
        }

        private void startControl()
        {
            if (active)
                report.EndControl();
            else
                report.StartControl(new ControlOverride());

            active = !active;
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
            if (active)
                report.EndControl();
        }

        private class ControlOverride : IManualTarget2
        {
            public Position2 Target { get; }
            public bool TriggerDown => true;
        }
    }
}

using System;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class ReportControlFactory : IReportControlFactory
    {
        private readonly IPulse pulse;

        public ReportControlFactory(IPulse pulse)
        {
            this.pulse = pulse;
        }

        public Control CreateForReport(IReport report, Disposer disposer, out double height)
        {
            var control = createForReport(report);
            height = control.Height;
            pulse.Heartbeat += control.Update;
            disposer.AddDisposable(new ReportDisposer(control, pulse));
            return control;
        }

        private static ReportControl createForReport(IReport report)
        {
            return report switch
            {
                IHealthReport healthReport => new HealthReportControl(healthReport),
                IStatisticsReport statisticsReport => new StatisticsReportControl(statisticsReport),

                _ => throw new InvalidOperationException($"Cannot create control for report {report}")
            };
        }

        private sealed class ReportDisposer : IDisposable
        {
            private readonly ReportControl control;
            private readonly IPulse pulse;

            public ReportDisposer(ReportControl control, IPulse pulse)
            {
                this.control = control;
                this.pulse = pulse;
            }

            public void Dispose()
            {
                control.Dispose();
                pulse.Heartbeat -= control.Update;
            }
        }
    }
}

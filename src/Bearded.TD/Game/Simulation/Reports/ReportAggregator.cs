using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Reports
{
    static partial class ReportAggregator
    {
        public static void Register(ComponentEvents events, IReport report)
        {
            events.Send(new ReportAdded(report));
            events.Subscribe(new GatherReportsListener(report));
        }

        private sealed class GatherReportsListener : IListener<GatherReports>
        {
            private readonly IReport report;

            public GatherReportsListener(IReport report)
            {
                this.report = report;
            }

            public void HandleEvent(GatherReports @event)
            {
                @event.AddReport(report);
            }
        }

        public static void AggregateForever(ComponentEvents events, Action<IReport> reportConsumer)
        {
            var existingReports = new List<IReport>();
            events.Send(new GatherReports(existingReports));
            existingReports.ForEach(reportConsumer);
            events.Subscribe(new AddReportListener(reportConsumer));
        }

        private sealed class AddReportListener : IListener<ReportAdded>
        {
            private readonly Action<IReport> reportConsumer;

            public AddReportListener(Action<IReport> reportConsumer)
            {
                this.reportConsumer = reportConsumer;
            }

            public void HandleEvent(ReportAdded @event)
            {
                reportConsumer(@event.Report);
            }
        }
    }
}

using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Reports;

static partial class ReportAggregator
{
    public static IReportHandle Register(ComponentEvents events, IReport report)
    {
        events.Send(new ReportAdded(report));
        var listener = new GatherReportsListener(report);
        events.Subscribe(listener);
        return new ReportHandle(events, report, listener);
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

    public static void AggregateForever(ComponentEvents events, IReportConsumer reportConsumer)
    {
        var existingReports = new List<IReport>();
        events.Send(new GatherReports(existingReports));
        existingReports.ForEach(reportConsumer.OnReportAdded);
        var reportListener = new ReportListener(reportConsumer);
        events.Subscribe<ReportAdded>(reportListener);
        events.Subscribe<ReportRemoved>(reportListener);
    }

    private sealed class ReportListener : IListener<ReportAdded>, IListener<ReportRemoved>
    {
        private readonly IReportConsumer reportConsumer;

        public ReportListener(IReportConsumer reportConsumer)
        {
            this.reportConsumer = reportConsumer;
        }

        public void HandleEvent(ReportAdded @event)
        {
            reportConsumer.OnReportAdded(@event.Report);
        }

        public void HandleEvent(ReportRemoved @event)
        {
            reportConsumer.OnReportRemoved(@event.Report);
        }
    }
}
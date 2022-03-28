using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Reports;

static partial class ReportAggregator
{
    public interface IReportHandle
    {
        void Unregister();
    }

    private sealed class ReportHandle : IReportHandle
    {
        private readonly ComponentEvents events;
        private readonly IReport report;
        private readonly GatherReportsListener listener;

        public ReportHandle(ComponentEvents events, IReport report, GatherReportsListener listener)
        {
            this.events = events;
            this.report = report;
            this.listener = listener;
        }

        public void Unregister()
        {
            events.Unsubscribe(listener);
            events.Send(new ReportRemoved(report));
        }
    }
}
namespace Bearded.TD.Game.Simulation.Reports;

static partial class ReportAggregator
{
    public interface IReportConsumer
    {
        void OnReportAdded(IReport report);
        void OnReportRemoved(IReport report);
    }
}
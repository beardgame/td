using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Debug;

sealed class DebugReporter : Component
{
    private IIdProvider? idProvider;
    private IProperty<Temperature>? temperature;

    protected override void OnAdded()
    {
        ReportAggregator.Register(Events, new DebugReport(this));

        ComponentDependencies.Depend<IIdProvider>(Owner, Events, provider => idProvider = provider);
        ComponentDependencies.Depend<IProperty<Temperature>>(Owner, Events, property => temperature = property);
    }

    public override void Update(TimeSpan elapsedTime) {}

    private sealed class DebugReport : IDebugReport
    {
        private readonly DebugReporter subject;

        public ReportType Type => ReportType.Debug;

        public string Id => subject.idProvider?.Id.ToString() ?? "NONE";
        public string Temperature => subject.temperature?.Value.ToString() ?? "NONE";

        public DebugReport(DebugReporter subject)
        {
            this.subject = subject;
        }
    }
}

interface IDebugReport : IReport
{
    public string Id { get; }
    public string Temperature { get; }
}

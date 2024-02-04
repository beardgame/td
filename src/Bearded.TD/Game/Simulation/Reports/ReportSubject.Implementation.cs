using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Reports;

sealed class ReportSubject : Component, IReportSubject, ReportAggregator.IReportConsumer
{
    private readonly SortedSet<IReport> reports =
        new(Utilities.Comparer
            .Comparing<IReport, byte>(r => (byte) r.Type)
            .ThenComparing(r => r.GetHashCode()));

    private IObjectAttributes attributes = ObjectAttributes.Default;
    private IFactionProvider? factionProvider;

    public string Name => attributes.Name;
    public Faction? Faction => factionProvider?.Faction;
    public IReadOnlyCollection<IReport> Reports => ImmutableArray.CreateRange(reports);

    public event VoidEventHandler? ReportsUpdated;

    protected override void OnAdded()
    {
        ReportAggregator.AggregateForever(Events, this);
        ComponentDependencies.Depend<IObjectAttributes>(Owner, Events, attr => attributes = attr);
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider => factionProvider = provider);
    }

    public void OnReportAdded(IReport report)
    {
        reports.Add(report);
        ReportsUpdated?.Invoke();
    }

    public void OnReportRemoved(IReport report)
    {
        reports.Remove(report);
        ReportsUpdated?.Invoke();
    }

    public override void Update(TimeSpan elapsedTime) { }
}

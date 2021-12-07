using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Reports
{
    sealed class ReportSubject<T> : Component<T>, IReportSubject, ReportAggregator.IReportConsumer
        where T : IComponentOwner
    {
        private readonly SortedSet<IReport> reports =
            new(Utilities.Comparer<IReport>.Comparing<IReport, byte>(r => (byte) r.Type));

        private INameProvider? nameProvider;
        private IFactionProvider? factionProvider;

        public string Name => nameProvider.NameOrDefault();
        public Faction? Faction => factionProvider?.Faction;
        public IReadOnlyCollection<IReport> Reports => ImmutableArray.CreateRange(reports);

        public event VoidEventHandler? ReportsUpdated;

        protected override void OnAdded()
        {
            ReportAggregator.AggregateForever(Events, this);
            ComponentDependencies.Depend<INameProvider>(Owner, Events, provider => nameProvider = provider);
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
}

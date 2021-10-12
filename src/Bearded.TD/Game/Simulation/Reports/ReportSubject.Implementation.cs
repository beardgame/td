using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Reports
{
    sealed class ReportSubject<T> : Component<T>, IReportSubject, ReportAggregator.IReportConsumer
        where T : IComponentOwner, INamed
    {
        private readonly SortedSet<IReport> reports =
            new(Utilities.Comparer<IReport>.Comparing<IReport, byte>(r => (byte) r.Type));

        private IFactionProvider? factionProvider;

        public string Name => Owner.Name;
        public Faction? Faction => factionProvider?.Faction;
        public IReadOnlyCollection<IReport> Reports => ImmutableArray.CreateRange(reports);

        protected override void OnAdded()
        {
            ReportAggregator.AggregateForever(Events, this);
            ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider => factionProvider = provider);
        }

        public void OnReportAdded(IReport report)
        {
            reports.Add(report);
        }

        public void OnReportRemoved(IReport report)
        {
            reports.Remove(report);
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(CoreDrawers drawers) { }
    }
}

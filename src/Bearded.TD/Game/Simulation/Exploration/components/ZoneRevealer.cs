using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Exploration
{
    sealed class ZoneRevealer<T> : Component<T> where T : IGameObject
    {
        private readonly Zone zone;

        public ZoneRevealer(Zone zone)
        {
            this.zone = zone;
        }

        protected override void OnAdded()
        {
            ReportAggregator.Register(Events, new ZoneRevealReport(this));
        }

        public override void Update(TimeSpan elapsedTime) {}

        private sealed class ZoneRevealReport : IZoneRevealReport
        {
            private readonly ZoneRevealer<T> zoneRevealer;

            public Zone Zone => zoneRevealer.zone;
            public bool CanRevealNow => zoneRevealer.Owner.Game.ExplorationManager.HasExplorationToken;
            public ReportType Type => ReportType.EntityActions;

            public ZoneRevealReport(ZoneRevealer<T> zoneRevealer)
            {
                this.zoneRevealer = zoneRevealer;
            }
        }
    }
}

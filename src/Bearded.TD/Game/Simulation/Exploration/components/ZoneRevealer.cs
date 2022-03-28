using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Exploration;

sealed class ZoneRevealer : Component
{
    private readonly Zone zone;

    public ZoneRevealer(Zone zone)
    {
        this.zone = zone;
    }

    protected override void OnAdded()
    {
        ReportAggregator.Register(Events, new ZoneRevealReport(this));
        Owner.Game.ListAs<IZoneRevealer>(new ZoneRevealerProxy(Owner));
    }

    public override void Update(TimeSpan elapsedTime) {}

    private sealed class ZoneRevealReport : IZoneRevealReport
    {
        private readonly ZoneRevealer zoneRevealer;

        public Zone Zone => zoneRevealer.zone;
        public bool CanRevealNow => zoneRevealer.Owner.Game.ExplorationManager.HasExplorationToken;
        public ReportType Type => ReportType.EntityActions;

        public ZoneRevealReport(ZoneRevealer zoneRevealer)
        {
            this.zoneRevealer = zoneRevealer;
        }
    }

    sealed class ZoneRevealerProxy : IZoneRevealer
    {
        private readonly GameObject owner;

        public Position3 Position => owner.Position;
        public bool Deleted => owner.Deleted;

        public ZoneRevealerProxy(GameObject owner)
        {
            this.owner = owner;
        }
    }
}

interface IZoneRevealer : IPositionable, IDeletable {}

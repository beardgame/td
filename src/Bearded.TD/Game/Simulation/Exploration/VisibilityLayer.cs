using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Exploration.events;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Exploration
{
    sealed class VisibilityLayer
    {
        private readonly GlobalGameEvents events;
        private readonly ZoneLayer zoneLayer;
        private readonly HashSet<Zone> revealedZones = new();

        public VisibilityLayer(GlobalGameEvents events, ZoneLayer zoneLayer)
        {
            this.events = events;
            this.zoneLayer = zoneLayer;
        }

        public void RevealAllZones()
        {
            foreach (var zone in zoneLayer.AllZones)
            {
                RevealZone(zone);
            }
        }

        public void RevealZone(Zone zone)
        {
            if (revealedZones.Add(zone))
            {
                events.Send(new ZoneRevealed(zone));
            }
        }

        public TileVisibility this[Tile tile]
        {
            get
            {
                var zone = zoneLayer.ZoneForTile(tile);
                if (zone == null)
                {
                    return TileVisibility.Revealed;
                }

                return revealedZones.Contains(zone) ? TileVisibility.Visible : TileVisibility.Invisible;
            }
        }
    }
}

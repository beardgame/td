using System;
using System.Collections.Generic;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Zones
{
    sealed class ZoneLayer
    {
        private readonly IdDictionary<Zone> zonesById = new();
        private readonly Tilemap<Zone?> zonesByTile;

        public IEnumerable<Zone> AllZones => zonesById.Values;

        public ZoneLayer(int radius)
        {
            zonesByTile = new Tilemap<Zone>(radius);
        }

        public void AddZone(Zone zone)
        {
            zonesById.Add(zone);
            foreach (var tile in zone.Tiles)
            {
                DebugAssert.State.Satisfies(() => zonesByTile[tile] == null);
                zonesByTile[tile] = zone;
            }
        }

        public Zone FindZone(Id<Zone> id) => zonesById[id];

        public Zone? ZoneForTile(Tile tile) => zonesByTile[tile];

        public IEnumerable<Zone> AdjacentTiles(Zone zone) => throw new NotImplementedException();
    }
}

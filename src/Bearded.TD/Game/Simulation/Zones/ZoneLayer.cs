using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Zones;

sealed class ZoneLayer
{
    private readonly IdDictionary<Zone> zonesById = new();
    private readonly Tilemap<ImmutableArray<Zone>> zonesByTile;
    private readonly MultiDictionary<Zone, Zone> adjacentZones = new();

    public IEnumerable<Zone> AllZones => zonesById.Values;

    public ZoneLayer(int radius)
    {
        zonesByTile = new Tilemap<ImmutableArray<Zone>>(radius, _ => ImmutableArray<Zone>.Empty);
    }

    public void AddZone(Zone zone)
    {
        zonesById.Add(zone);
        foreach (var tile in zone.Tiles)
        {
            zonesByTile[tile] = zonesByTile[tile].Add(zone);
        }
    }

    public void ConnectZones(Zone from, Zone to)
    {
        adjacentZones.Add(from, to);
        adjacentZones.Add(to, from);
    }

    public Zone FindZone(Id<Zone> id) => zonesById[id];

    public IEnumerable<Zone> ZonesForTile(Tile tile) => zonesByTile[tile];

    public IEnumerable<Zone> AdjacentZones(Zone zone) => adjacentZones[zone];
}

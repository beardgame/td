using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Zones;

sealed class ZoneLayer
{
    private readonly IdDictionary<Zone> zonesById = new();
    private readonly Tilemap<Zone?> zonesByTile;
    private readonly Tilemap<ImmutableArray<Zone>> zonesForVisibilityByTile;
    private readonly MultiDictionary<Zone, Zone> adjacentZones = new();

    public IEnumerable<Zone> AllZones => zonesById.Values;

    public ZoneLayer(int radius)
    {
        zonesByTile = new Tilemap<Zone>(radius);
        zonesForVisibilityByTile = new Tilemap<ImmutableArray<Zone>>(radius, _ => ImmutableArray<Zone>.Empty);
    }

    public void AddZone(Zone zone)
    {
        zonesById.Add(zone);
        foreach (var tile in zone.CoreTiles)
        {
            DebugAssert.State.Satisfies(zonesByTile[tile] == null);
            zonesByTile[tile] = zonesByTile[tile] = zone;
        }
        foreach (var tile in zone.VisibilityTiles)
        {
            zonesForVisibilityByTile[tile] = zonesForVisibilityByTile[tile].Add(zone);
        }
    }

    public void ConnectZones(Zone from, Zone to)
    {
        adjacentZones.Add(from, to);
        adjacentZones.Add(to, from);
    }

    public Zone FindZone(Id<Zone> id) => zonesById[id];

    public Zone? ZoneForTile(Tile tile) => zonesByTile[tile];

    public ImmutableArray<Zone> ZonesForVisibilityForTile(Tile tile) => zonesForVisibilityByTile[tile];

    public IEnumerable<Zone> AdjacentZones(Zone zone) => adjacentZones[zone];

    // Used only by debug commands to reset the map so it can be regenerated.
    public void ResetZones()
    {
        var ids = zonesById.Keys.ToImmutableArray();

        foreach (var id in ids)
        {
            zonesById.Remove(id);
        }

        foreach (var t in Tilemap.EnumerateTilemapWith(zonesByTile.Radius))
        {
            zonesByTile[t] = null;
            zonesForVisibilityByTile[t] = ImmutableArray<Zone>.Empty;
        }

        adjacentZones.Clear();
    }
}

using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Exploration;

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

    public bool RevealZone(Zone zone)
    {
        if (!revealedZones.Add(zone))
        {
            return false;
        }

        events.Send(new ZoneRevealed(zone));
        return true;

    }

    public ZoneVisibility this[Zone zone] =>
        revealedZones.Contains(zone) ? ZoneVisibility.Revealed : ZoneVisibility.Invisible;

    public TileVisibility this[Tile tile]
    {
        get
        {
            var zones = zoneLayer.ZonesForTile(tile);
            return zones.Any(revealedZones.Contains) ? TileVisibility.Visible : TileVisibility.Invisible;
        }
    }
}

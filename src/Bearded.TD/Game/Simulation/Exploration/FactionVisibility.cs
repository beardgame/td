using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Exploration;

[FactionBehavior("visibility")]
sealed class FactionVisibility : FactionBehavior<Faction>
{
    private readonly ZoneLayer zoneLayer;
    private readonly HashSet<Zone> revealedZones = new();

    public FactionVisibility(ZoneLayer zoneLayer)
    {
        this.zoneLayer = zoneLayer;
    }

    protected override void Execute()
    {
        throw new System.NotImplementedException();
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

        Events.Send(new ZoneRevealed(zone));
        return true;
    }

    public ZoneVisibility this[Zone zone] =>
        revealedZones.Contains(zone) ? ZoneVisibility.Revealed : ZoneVisibility.Invisible;

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

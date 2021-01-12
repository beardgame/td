using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Workers
{
    readonly struct TileMined : IGlobalEvent
    {
        public Faction Faction { get; }
        public Tile Tile { get; }

        public TileMined(Faction faction, Tile tile)
        {
            Faction = faction;
            Tile = tile;
        }
    }
}

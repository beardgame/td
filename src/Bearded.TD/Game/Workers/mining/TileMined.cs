using Bearded.TD.Game.Events;
using Bearded.TD.Game.Factions;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Workers
{
    struct TileMined : IGlobalEvent
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

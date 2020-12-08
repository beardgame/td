using Bearded.TD.Game.GameState.Events;
using Bearded.TD.Game.GameState.Factions;
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

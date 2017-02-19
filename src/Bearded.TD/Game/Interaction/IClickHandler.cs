using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;

namespace Bearded.TD.Game.Interaction
{
    interface IClickHandler
    {
        Footprint Footprint { get; }

        void HandleHover(GameState game, Tile<TileInfo> rootTile);
        void HandleClick(GameState game, Tile<TileInfo> rootTile);

        void Enable(GameState game);
        void Disable(GameState game);
    }
}

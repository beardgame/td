using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;

namespace Bearded.TD.Game.Interaction
{
    interface IClickHandler
    {
        TileSelection Selection { get; }

        void HandleHover(GameState game, PositionedFootprint footprint);
        void HandleClick(GameState game, PositionedFootprint footprint);

        void Enable(GameState game);
        void Disable(GameState game);
    }
}

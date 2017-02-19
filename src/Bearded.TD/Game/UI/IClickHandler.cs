using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.UI
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

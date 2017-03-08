using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.UI
{
    interface IClickHandler
    {
        TileSelection Selection { get; }

        void HandleHover(GameInstance game, PositionedFootprint footprint);
        void HandleClick(GameInstance game, PositionedFootprint footprint);

        void Enable(GameInstance game);
        void Disable(GameInstance game);
    }
}

using Bearded.TD.Game.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.UI
{
    class CursorState
    {
        private readonly GameInstance game;
        public IClickHandler ClickHandler { get; private set; }

        public CursorState(GameInstance game)
        {
            this.game = game;
        }

        public void SetClickHandler(IClickHandler newHandler)
        {
            if (newHandler == ClickHandler) return;

            ClickHandler?.Disable(game);
            ClickHandler = newHandler;
            newHandler?.Enable(game);
        }

        public PositionedFootprint GetFootprintForPosition(Position2 position)
        {
            return ClickHandler.Selection.GetPositionedFootprint(game.State.Level, position);
        }

        public void Hover(PositionedFootprint footprint)
        {
            ClickHandler?.HandleHover(game, footprint);
        }

        public void Click(PositionedFootprint footprint)
        {
            ClickHandler?.HandleClick(game, footprint);
        }
    }
}

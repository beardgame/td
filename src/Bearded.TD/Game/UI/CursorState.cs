using Bearded.TD.Game.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.UI
{
    class CursorState
    {
        private readonly GameState gameState;
        public IClickHandler ClickHandler { get; private set; }

        public CursorState(GameState gameState)
        {
            this.gameState = gameState;
        }

        public void SetClickHandler(IClickHandler newHandler)
        {
            if (newHandler == ClickHandler) return;

            ClickHandler?.Disable(gameState);
            ClickHandler = newHandler;
            newHandler?.Enable(gameState);
        }

        public PositionedFootprint GetFootprintForPosition(Position2 position)
        {
            return ClickHandler.Selection.GetPositionedFootprint(gameState.Level, position);
        }

        public void Hover(PositionedFootprint footprint)
        {
            ClickHandler?.HandleHover(gameState, footprint);
        }

        public void Click(PositionedFootprint footprint)
        {
            ClickHandler?.HandleClick(gameState, footprint);
        }
    }
}

using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.UI
{
    class MouseCursorHandler : ICursorHandler
    {
        private readonly Level level;
        private TileSelection tileSelection;
        private Position2 mousePosition;

        public IAction ClickAction { get; private set; }
        public PositionedFootprint CurrentFootprint { get; private set; }

        public MouseCursorHandler(Level level)
        {
            this.level = level;
            tileSelection = TileSelection.FromFootprints(FootprintGroup.Single);
        }

        public void Update(UpdateEventArgs args, GameInputContext inputContext)
        {
            mousePosition = inputContext.MousePosition;
            ClickAction = inputContext.ClickAction;
            updateFootprint();
        }

        public void SetTileSelection(TileSelection tileSelection)
        {
            this.tileSelection = tileSelection;
            updateFootprint();
        }

        private void updateFootprint()
        {
            CurrentFootprint = tileSelection.GetPositionedFootprint(level, mousePosition);
        }
    }
}

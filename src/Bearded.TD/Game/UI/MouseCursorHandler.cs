using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;

namespace Bearded.TD.Game.UI
{
    class MouseCursorHandler : ICursorHandler
    {
        private readonly Level level;
        private TileSelection tileSelection;

        public PositionedFootprint CurrentFootprint { get; private set; }

        public MouseCursorHandler(Level level)
        {
            this.level = level;
            tileSelection = TileSelection.FromFootprints(FootprintGroup.Single);
        }

        public void Update(UpdateEventArgs args, GameInputContext inputContext)
        {
            CurrentFootprint = tileSelection.GetPositionedFootprint(level, inputContext.MousePos);
        }

        public void SetTileSelection(TileSelection tileSelection)
        {
            this.tileSelection = tileSelection;
        }
    }
}

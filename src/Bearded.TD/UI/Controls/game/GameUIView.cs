using Bearded.TD.Rendering;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class GameUIView : CompositeControl
    {
        private readonly GameUI gameUI;

        public GameUIView(GameUI gameUI, FrameCompositor compositor, GeometryManager geometryManager)
        {
            this.gameUI = gameUI;

            Add(new ActionBarView(gameUI.ActionBar)
                .Anchor(a => a
                    .Left(width: 160)
                    .Top(margin: -200, height: 400, relativePercentage: .5)));
            Add(new GameWorldControl(gameUI.Game, compositor, geometryManager));
        }
    }
}

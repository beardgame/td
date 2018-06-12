using Bearded.TD.Rendering;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class GameUIView : CompositeControl
    {
        public GameUIView(GameUI gameUI, FrameCompositor compositor, GeometryManager geometryManager)
        {
            Add(new ActionBarView(gameUI.ActionBar)
                .Anchor(a => a
                    .Left(width: 160)
                    .Top(margin: -200, height: 400, relativePercentage: .5)));
            Add(new GameWorldView(gameUI.Game, compositor, geometryManager));
        }
    }
}

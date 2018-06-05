using Bearded.TD.Rendering;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class GameUIView : CompositeControl
    {
        public GameUIView(GameUI gameUI, FrameCompositor compositor, GeometryManager geometryManager)
        {
            Add(new GameWorldView(gameUI.Game, compositor, geometryManager));
        }
    }
}

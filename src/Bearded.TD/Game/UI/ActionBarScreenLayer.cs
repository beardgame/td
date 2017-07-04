using Bearded.TD.Game.UI.Components;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.TD.UI;
using OpenTK;

namespace Bearded.TD.Game.UI
{
    class ActionBarScreenLayer : UIScreenLayer
    {
        public ActionBarScreenLayer(ScreenLayerCollection parent, GeometryManager geometries)
            : base(parent, geometries)
        {
            AddComponent(new ActionBar(Bounds.AnchoredBox(Screen, 0, .5f, new Vector2(120, 320))));
        }
    }
}

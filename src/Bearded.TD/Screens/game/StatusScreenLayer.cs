using Bearded.TD.Game;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using OpenTK;

namespace Bearded.TD.Screens
{
    class StatusScreenLayer : UIScreenLayer
    {
        public StatusScreenLayer(ScreenLayerCollection parent, GeometryManager geometries, GameInstance game)
            : base(parent, geometries)
        {
            AddComponent(new FactionStatus(
                Bounds.AnchoredBox(Screen, BoundsAnchor.End, BoundsAnchor.Start, new Vector2(160, 100)),
                game.Me.Faction));
        }
    }
}

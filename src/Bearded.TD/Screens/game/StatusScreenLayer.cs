using Bearded.TD.Game;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using static Bearded.TD.UI.BoundsConstants;

namespace Bearded.TD.Screens
{
    class StatusScreenLayer : UIScreenLayer
    {
        public StatusScreenLayer(ScreenLayerCollection parent, GeometryManager geometries, GameInstance game)
            : base(parent, geometries)
        {
            AddComponent(new FactionStatus(
                Bounds.AnchoredBox(Screen, TopRight, Size(160, 100)),
                game.Me.Faction));
            AddComponent(new PlayerStatus(
                Bounds.AnchoredBox(Screen, BottomRight, Size(160, 100)),
                game));
        }
    }
}

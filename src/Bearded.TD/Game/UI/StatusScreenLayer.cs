using Bearded.TD.Game.UI.Components;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.TD.UI;
using OpenTK;

namespace Bearded.TD.Game.UI
{
    class StatusScreenLayer : UIScreenLayer
    {
        private readonly GameInstance game;

        public StatusScreenLayer(ScreenLayerCollection parent, GeometryManager geometries, GameInstance game)
            : base(parent, geometries)
        {
            AddComponent(new FactionStatus(
                Bounds.AnchoredBox(Screen, BoundsAnchor.End, BoundsAnchor.Start, new Vector2(160, 100)),
                game.Me.Faction));
        }
    }
}

using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;

namespace Bearded.TD.Game.UI.Components
{
    class DebugGenerationInfo : FocusableUIComponent
    {
        private readonly GameInstance game;

        public DebugGenerationInfo(Bounds bounds, GameInstance game) : base(bounds)
        {
            this.game = game;
        }

        public override void Draw(GeometryManager geometries)
        {
            if (!IsFocused) return;
            var bgGeo = geometries.ConsoleBackground;
            bgGeo.Color = Color.Black * .7f;
            bgGeo.DrawRectangle(Bounds.Start(), Bounds.Size());
        }
    }
}

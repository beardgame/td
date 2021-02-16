using Bearded.Graphics;
using Bearded.Graphics.Text;
using Bearded.TD.UI.Controls;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.UI
{
    sealed class LabelRenderer : IRenderer<Label>
    {
        private readonly TextDrawerWithDefaults<Color> textDrawer;

        public LabelRenderer(TextDrawerWithDefaults<Color> textDrawer)
        {
            this.textDrawer = textDrawer;
        }

        public void Render(Label label)
        {
            var argb = label.Color;

            if (label.Parent is Button button && !button.IsEnabled)
                argb *= 0.5f;

            var (anchorX, anchorY) = label.TextAnchor;
            var frame = label.Frame;
            var anchor = frame.TopLeft + frame.Size * label.TextAnchor;

            textDrawer.DrawLine(
                xyz: ((Vector2) anchor).WithZ(),
                text: label.Text,
                alignHorizontal: (float) anchorX,
                alignVertical: (float) anchorY,
                fontHeight: (float) label.FontSize,
                parameters: argb
            );
        }
    }
}

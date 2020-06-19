using amulware.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenToolkit.Mathematics;

namespace Bearded.TD.Rendering.UI
{
    class LabelRenderer : IRenderer<Label>
    {
        private readonly FontGeometry geometry;

        public LabelRenderer(IndexedSurface<UVColorVertexData> surface, Font font)
        {
            geometry = new FontGeometry(surface, font)
            {
                Color = Color.White,
            };
        }

        public void Render(Label label)
        {
            geometry.Height = (float)label.FontSize;
            var argb = label.Color;

            if (label.Parent is Button button && !button.IsEnabled)
                argb *= 0.5f;

            geometry.Color = argb;

            var textAnchor = label.TextAnchor;
            var frame = label.Frame;
            var anchor = frame.TopLeft + frame.Size * label.TextAnchor;

            geometry.DrawString((Vector2)anchor, label.Text, (float)textAnchor.X, (float)textAnchor.Y);
        }
    }
}

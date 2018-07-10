using amulware.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK;

namespace Bearded.TD.Rendering.UI
{
    sealed class BackgroundBoxRenderer : IRenderer<BackgroundBox>
    {
        private readonly PrimitiveGeometry geometry;

        public BackgroundBoxRenderer(IndexedSurface<PrimitiveVertexData> surface)
        {
            geometry = new PrimitiveGeometry(surface);
        }

        public void Render(BackgroundBox control)
        {
            var frame = control.Frame;
            geometry.Color = control.Color;

            geometry.DrawRectangle((Vector2)frame.TopLeft, (Vector2)frame.Size);
        }
    }
}

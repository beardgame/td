using amulware.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using OpenToolkit.Mathematics;

namespace Bearded.TD.Rendering.UI
{
    sealed class DotRenderer : IRenderer<Dot>
    {
        private readonly PrimitiveGeometry geometry;

        public DotRenderer(IndexedSurface<PrimitiveVertexData> surface)
        {
            geometry = new PrimitiveGeometry(surface);
        }

        public void Render(Dot control)
        {
            var frame = control.Frame;
            geometry.Color = control.Color;

            geometry.DrawOval((Vector2) frame.TopLeft, (Vector2) frame.Size, filled: true);
        }
    }
}

using amulware.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    class Dot : Control
    {
        private static readonly Color defaultColor = Color.White;

        public virtual Color Color { get; }

        public Dot(Color? color = null)
        {
            Color = color ?? defaultColor;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}

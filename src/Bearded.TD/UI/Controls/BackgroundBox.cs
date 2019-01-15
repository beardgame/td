using amulware.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class BackgroundBox : CompositeControl
    {
        public Color Color = Color.Black * .75f;

        public BackgroundBox() { }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}


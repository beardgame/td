using amulware.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class BackgroundBox : CompositeControl
    {
        public Color Color { get; set; } = Color.Black * .75f;

        public BackgroundBox(Color? color = null)
        {
            if(color.HasValue)
                Color = color.Value;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}


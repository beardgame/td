using Bearded.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class BackgroundBox : CompositeControl
    {
        public static readonly Color DefaultColor = Color.Black * .75f;

        public Color Color { get; set; } = DefaultColor;

        public BackgroundBox(Color? color = null)
        {
            if(color.HasValue)
                Color = color.Value;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}


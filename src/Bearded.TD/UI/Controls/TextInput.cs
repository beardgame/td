using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    class TextInput : Bearded.UI.Controls.TextInput
    {
        public double FontSize { get; set; } = 24;

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}

using Bearded.UI.Rendering;

namespace Bearded.UI.Controls
{
    public sealed class SimpleControl : Control
    {
        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}

using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

sealed class EmptyControl : Control
{
    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}

using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

sealed class BlurBackground : Control
{
    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}

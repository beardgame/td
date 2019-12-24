using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    class Border : Control
    {
        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}

using Bearded.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

sealed class Border : Control
{
    public Color Color { get; set; } = Constants.UI.Colors.Get(ForeGroundColor.Highlight);

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}

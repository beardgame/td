using Bearded.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

sealed class Border : Control
{
    private static readonly Color defaultColor = Color.White;

    public Color Color { get; set; } = defaultColor;

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}

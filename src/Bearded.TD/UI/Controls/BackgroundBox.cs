using Bearded.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

sealed class BackgroundBox(Color? color = null) : CompositeControl
{
    public Color Color { get; set; } = color ?? Constants.UI.Colors.Get(BackgroundColor.Default);

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}

using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Shapes;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

sealed class DropShadow : Control
{
    public Control? SourceControl { get; set; }
    public double CornerRadius => (SourceControl as ICornerRadiusSource)?.CornerRadius ?? 0;

    public ShapeComponents OverlayComponents { get; set; }

    public Shadow Shadow { get; set; }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}

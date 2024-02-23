using Bearded.Graphics;
using Bearded.TD.Rendering.Shapes;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

sealed class ComplexBox : Control, ICornerRadiusSource
{
    public double CornerRadius { get; set; }

    public float EdgeOuterWidth { get; set; }
    public float EdgeInnerWidth { get; set; }
    public float GlowOuterWidth { get; set; }
    public float GlowInnerWidth { get; set; }

    public Color FillColor { get; set; }
    public Color EdgeColor { get; set; }
    public Color GlowOuterColor { get; set; }
    public Color GlowInnerColor { get; set; }

    public EdgeData Edges => new(EdgeOuterWidth, EdgeInnerWidth, GlowOuterWidth, GlowInnerWidth);
    public ShapeColors Colors => new(FillColor, EdgeColor, GlowOuterColor, GlowInnerColor);

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}

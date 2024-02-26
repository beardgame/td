using System;
using Bearded.Graphics;
using Bearded.TD.Rendering.Shapes;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using static Bearded.TD.Rendering.Shapes.Shapes;

namespace Bearded.TD.UI.Controls;

abstract class ComplexShapeControl : Control, ICornerRadiusSource
{
    public virtual double CornerRadius { get; set; }

    public float EdgeOuterWidth { get; set; }
    public float EdgeInnerWidth { get; set; }
    public float GlowOuterWidth { get; set; }
    public float GlowInnerWidth { get; set; }

    public Color FillColor { get; set; }
    public Color EdgeColor { get; set; }
    public Color GlowOuterColor { get; set; }
    public Color GlowInnerColor { get; set; }

    public EdgeData Edges
    {
        get => new(EdgeOuterWidth, EdgeInnerWidth, GlowOuterWidth, GlowInnerWidth);
        set => (EdgeOuterWidth, EdgeInnerWidth, GlowOuterWidth, GlowInnerWidth) =
            (value.OuterWidth, value.InnerWidth, value.OuterGlow, value.InnerGlow);
    }

    public ShapeColors Colors
    {
        get => new(FillColor, EdgeColor, GlowOuterColor, GlowInnerColor);
        set => (FillColor, EdgeColor, GlowOuterColor, GlowInnerColor) =
            (value.Fill, value.Edge, value.OuterGlow, value.InnerGlow);
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}

sealed class ComplexBox : ComplexShapeControl;

sealed class ComplexCircle : ComplexShapeControl
{
    public override double CornerRadius
    {
        get => Math.Min(Frame.Size.X, Frame.Size.Y);
        set => throw new InvalidOperationException("Cannot set corner radius of circle.");
    }

    public Circle Shape => Circle(Frame.TopLeft + Frame.Size * 0.5, Math.Min(Frame.Size.X, Frame.Size.Y) * 0.5);
}

using System;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Shapes;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using static Bearded.TD.Rendering.Shapes.Shapes;

namespace Bearded.TD.UI.Controls;


abstract class ComplexShapeControl : Control, ICornerRadiusSource
{
    public virtual double CornerRadius { get; set; }

    public Fill Fill { get; set; }
    public Edge Edge { get; set; }
    public Glow OuterGlow { get; set; }
    public Glow InnerGlow { get; set; }

    public ShapeComponents Components
    {
        get => new(Fill, Edge, OuterGlow, InnerGlow);
        set => (Fill, Edge, OuterGlow, InnerGlow) = value;
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

using Bearded.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Controls;

sealed class BoxShadow : Control
{
    private ICornerRadiusSource? cornerRadiusSource;
    private double cornerRadius;

    public double CornerRadius => cornerRadiusSource?.CornerRadius ?? cornerRadius;

    public Vector3d Offset { get; set; }
    public double BlurRadius { get; set; }
    public Color Color { get; set; }

    public BoxShadow WithCornerRadiusSource(ICornerRadiusSource source)
    {
        cornerRadiusSource = source;
        return this;
    }

    public BoxShadow WithCornerRadius(double radius)
    {
        cornerRadius = radius;
        cornerRadiusSource = null;
        return this;
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}

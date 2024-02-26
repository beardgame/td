using Bearded.Graphics;
using Bearded.TD.Rendering.Shapes;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;
using static Bearded.TD.Rendering.Shapes.Shapes;

namespace Bearded.TD.UI.Controls;

sealed class DropShadow : Control
{
    public Control? SourceControl { get; set; }
    public double CornerRadius => (SourceControl as ICornerRadiusSource)?.CornerRadius ?? 0;

    public Vector3d Offset { get; set; }
    public double BlurRadius { get; set; }
    public Color Color { get; set; }

    public Shadow Shadow
    {
        get => Shadow(Offset, BlurRadius, Color);
        set => (Offset, BlurRadius, Color) = (value.Offset, value.PenumbraRadius, value.Color);
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}

static class DropShadowExtensions
{
    public static Control[] WithDropShadow(
        this Control source, Vector3d? offset = null, double? blurRadius = null, Color? color = null)
        => source.WithDropShadow(Shadow(
            offset ?? Constants.UI.Shadows.Default.Offset,
            blurRadius ?? Constants.UI.Shadows.Default.PenumbraRadius,
            color ?? Constants.UI.Shadows.Default.Color)
        );

    public static Control[] WithDropShadow(this Control source, Shadow shadow)
    {
        var dropShadow = new DropShadow
        {
            SourceControl = source,
            Shadow = shadow,
        };
        return [dropShadow, source];
    }
}

using System.Diagnostics;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Shapes;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

readonly record struct BlurredBackground
{
    public static BlurredBackground Default => default;

    public BlurredBackground? If(bool condition) => condition ? this : null;
}

readonly record struct Decorations(
    (Shadow Shadow, ShapeComponents OverlayComponents)? Shadow = null,
    BlurredBackground? BlurredBackground = null
)
{
    public Decorations(Shadow? Shadow, BlurredBackground? BlurredBackground)
        : this(Shadow == null ? null : (Shadow.Value, ShapeComponents.Empty), BlurredBackground)
    {
    }
};

static class ComplexShapeControlExtensions
{
    public static Control[] WithDropShadow(
        this Control source, Shadow shadow, ShapeComponents overlayComponents = default)
    {
        var dropShadow = new DropShadow
        {
            SourceControl = source,
            Shadow = shadow,
            OverlayComponents = overlayComponents,
        };
        return [dropShadow, source];
    }

    public static Control[] WithBlurredBackground(this ComplexShapeControl source)
    {
        return source.WithDecorations(new Decorations(BlurredBackground: BlurredBackground.Default));
    }

    public static Control[] WithDecorations(this ComplexShapeControl source, Decorations decorations)
    {
        var controls = new Control[
            1 + ifNotNull(decorations.Shadow) + ifNotNull(decorations.BlurredBackground)
        ];

        var i = 0;

        if (decorations.BlurredBackground != null)
        {
            Debug.Assert(!source.Components.IsMutable,
                "Must create blurred background manually to support mutable components.");
            source.Components = [Fill.With(GradientDefinition.BlurredBackground()), ..source.Components];
            controls[i++] = new BlurBackground();
        }

        if (decorations.Shadow != null)
        {
            var (shadow, overlayComponents) = decorations.Shadow.Value;
            controls[i++] = new DropShadow
            {
                SourceControl = source,
                Shadow = shadow,
                OverlayComponents = overlayComponents,
            };
        }

        controls[i] = source;
        return controls;

        static int ifNotNull<T>(T? value) => value is not null ? 1 : 0;
    }
}

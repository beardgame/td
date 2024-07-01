using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Bearded.Graphics;
using Bearded.TD.UI.Shapes;
using Bearded.UI;
using OpenTK.Mathematics;
using static Bearded.TD.UI.Shapes.GradientType;

namespace Bearded.TD.Rendering.Shapes;

static class GradientUIExtensions
{
    public static ShapeComponentsForDrawing ForDrawingWith(
        this ShapeComponents components, IShapeComponentBuffer componentBuffer, IGradientBuffer gradients, Frame frame)
        => ShapeComponentsForDrawing.From(components.Components, componentBuffer, (gradients, frame));

    public static ShapeComponentsForDrawing ForDrawingAssumingNoGradients(
        this ShapeComponents components, IShapeComponentBuffer componentBuffer)
        => ShapeComponentsForDrawing.From(components.Components, componentBuffer);

    public static GradientParameters ForDrawingWith(this ShapeColor color, IGradientBuffer gradients, Frame frame)
    {
        var gradient = color.Gradient;
        var gradientId = gradient.Length == 0
            ? GradientId.None
            : gradients.AddGradient(gradient);
        return color.Definition.ForDrawing(gradientId, frame);
    }

    public static GradientParameters ForDrawingWithoutGradients(this ShapeColor color)
        => color.Definition.ForDrawing(GradientId.None, default);

    public static GradientParameters ForDrawing(this GradientDefinition def,  GradientId gradientId, Frame frame)
    {
        validateGradientParameters(def, gradientId);

        var parameters = def.Type switch
        {
            None => default,
            Constant => (encodeColor(def.Color), 0, 0, 0),
            BlurredBackground => (0, 0, 0, 0),
            Linear => v4(def.Point1.CalculatePointWithin(frame), def.Point2.CalculatePointWithin(frame)),
            RadialWithRadius => v4(def.Point1.CalculatePointWithin(frame), (def.Radius, 0)),
            RadialToPoint => v4(def.Point1.CalculatePointWithin(frame), def.Point2.CalculatePointWithin(frame)),
            AlongEdgeNormal => (0, 0, 0, 0),
            ArcAroundPoint => v4(def.Point1.CalculatePointWithin(frame), (def.StartAngle.Radians, def.Length.Radians)),
            _ => throw new ArgumentOutOfRangeException(nameof(def.Type)),
        };

        return new GradientParameters(def.Type, gradientId, def.Flags, def.BlendMode, parameters);

        static float encodeColor(Color color) => Unsafe.BitCast<Color, float>(color);
        static Vector4 v4(Vector2 a, Vector2 b) => new(a.X, a.Y, b.X, b.Y);
    }

    [Conditional("DEBUG")]
    private static void validateGradientParameters(GradientDefinition definition, GradientId gradientId)
    {
        var expectsGradient = definition.Type >= Linear;
        var gotGradient = !gradientId.IsNone;

        System.Diagnostics.Debug.Assert(expectsGradient == gotGradient,
            expectsGradient
                ? $"Gradient ID required for gradient type {definition.Type}."
                : $"Gradient ID not allowed for gradient type {definition.Type}."
        );
    }
}

using System;
using Bearded.TD.UI.Shapes;
using Bearded.UI;

namespace Bearded.TD.Rendering.Shapes;

readonly struct ShapeComponentsForDrawing
{
    private ShapeComponentsForDrawing(ShapeVertex.ShapeComponents components, float maxDistance, float minDistance, ShapeFlags flags)
    {
        Components = components;
        MaxDistance = maxDistance;
        MinDistance = minDistance;
        Flags = flags;
    }

    public ShapeVertex.ShapeComponents Components { get; }
    public float MaxDistance { get; }
    public float MinDistance { get; }
    public ShapeFlags Flags { get; }

    public ShapeComponentsForDrawing WithFlags(ShapeFlags flags)
        => new(Components, MaxDistance, MinDistance, flags);

    public static ShapeComponentsForDrawing From(
        ShapeComponent component,
        IShapeComponentBuffer componentBuffer,
        (IGradientBuffer, Frame)? gradientsInFrame = null,
        ShapeFlags flags = default)
    {
        var max = Math.Max(component.ZeroDistance, component.OneDistance);
        var min = Math.Min(component.ZeroDistance, component.OneDistance);
        var componentForDrawing = ShapeComponentForDrawing.From(component, gradientsInFrame);
        var id = componentBuffer.AddComponent(componentForDrawing);
        return new ShapeComponentsForDrawing(id, maxDistance: max, minDistance: min, flags);
    }

    public static ShapeComponentsForDrawing From(
        ReadOnlySpan<ShapeComponent> components,
        IShapeComponentBuffer componentBuffer,
        (IGradientBuffer, Frame)? gradientsInFrame = null,
        ShapeFlags flags = default)
    {
        if (components.Length == 0)
            return new ShapeComponentsForDrawing(default, 0, 0, flags);

        var min = float.PositiveInfinity;
        var max = float.NegativeInfinity;
        Span<ShapeComponentForDrawing> componentsForDrawing = stackalloc ShapeComponentForDrawing[components.Length];

        for (var i = 0; i < components.Length; i++)
        {
            var component = components[i];
            componentsForDrawing[i] = ShapeComponentForDrawing.From(component, gradientsInFrame);

            min = Math.Min(min, component.ZeroDistance);
            min = Math.Min(min, component.OneDistance);
            max = Math.Max(max, component.ZeroDistance);
            max = Math.Max(max, component.OneDistance);
        }

        var ids = componentBuffer.AddComponents(componentsForDrawing);
        return new ShapeComponentsForDrawing(ids, maxDistance: max, minDistance: min, flags);
    }

    public ShapeComponentsForDrawing WithAdjacent(ShapeComponentsForDrawing other)
    {
        return new ShapeComponentsForDrawing(
            Components.WithAdjacent(other.Components),
            MaxDistance,
            MinDistance,
            Flags
        );
    }
}

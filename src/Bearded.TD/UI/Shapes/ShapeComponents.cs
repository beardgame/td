using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Bearded.Graphics;

namespace Bearded.TD.UI.Shapes;

readonly partial struct ShapeComponents
{
    public static ShapeComponents Empty => default;

    private enum Type
    {
        None = 0,
        Immutable = 1,
        Mutable = 2,
    }

    private readonly Type type;

    private readonly ImmutableArray<ShapeComponent> immutableComponents;
    private readonly ShapeComponent[]? mutableComponents;

    public bool IsMutable => type == Type.Mutable;

    public ReadOnlySpan<ShapeComponent> Components => type switch
    {
        Type.None => ReadOnlySpan<ShapeComponent>.Empty,
        Type.Immutable => immutableComponents.AsSpan(),
        Type.Mutable => mutableComponents,
        _ => throw new ArgumentOutOfRangeException(),
    };

    private ShapeComponents(ImmutableArray<ShapeComponent> components)
    {
        type = Type.Immutable;
        immutableComponents = components;
    }

    private ShapeComponents(ShapeComponent[] components)
    {
        type = Type.Mutable;
        mutableComponents = components;
    }

    public static ShapeComponents From(ImmutableArray<ShapeComponent> components)
        => new(components);

    public static ShapeComponents FromMutable(ShapeComponent[] components)
        => new(components);

    public static implicit operator ShapeComponents(ShapeComponent components)
        => [components];

}

[CollectionBuilder(typeof(ShapeComponents), nameof(From))]
readonly partial struct ShapeComponents : IEnumerable<ShapeComponent>
{
    public static ShapeComponents From(ReadOnlySpan<ShapeComponent> components)
        => From(components.ToImmutableArray());

    IEnumerator<ShapeComponent> IEnumerable<ShapeComponent>.GetEnumerator() => throw doNotEnumerate();
    IEnumerator IEnumerable.GetEnumerator() => throw doNotEnumerate();
    private static NotSupportedException doNotEnumerate()
        => new("We implemented IEnumerable only for collection expression support.");

    public ReadOnlySpan<ShapeComponent>.Enumerator GetEnumerator() => Components.GetEnumerator();
}

readonly record struct ShapeComponent(float ZeroDistance, float OneDistance, ShapeColor Color);

static class Edge
{
    public static ShapeComponent Outer(float width, ShapeColor color) => new(0, width, color);
    public static ShapeComponent Inner(float width, ShapeColor color) => new(0, -width, color);
}

static class Glow
{
    public static ShapeComponent Outer(float width, Color color)
        => Edge.Outer(width, GradientDefinition.SimpleGlow(color));

    public static ShapeComponent OuterFilled(float width, Color color)
        => Edge.Outer(width, GradientDefinition.SimpleGlow(color).AddFlags(GradientFlags.ExtendNegative));

    public static ShapeComponent Inner(float width, Color color)
        => Edge.Inner(width, GradientDefinition.SimpleGlow(color));
}

static class Fill
{
    private const float zero = 0;
    private const float one = float.NegativeInfinity;

    public static ShapeComponent With(Color color)
        => With(GradientDefinition.Constant(color));

    public static ShapeComponent With(GradientDefinition.SingleColor color)
        => new(zero, one, color);

    public static ShapeComponent With(ShapeColor color)
        => new(zero, one, color);
}

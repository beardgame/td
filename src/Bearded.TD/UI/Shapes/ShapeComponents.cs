using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

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

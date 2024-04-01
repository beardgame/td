using System;
using System.Collections.Immutable;
using Bearded.Graphics;

namespace Bearded.TD.UI.Shapes;

readonly struct ShapeColor
{
    private enum Type
    {
        Constant = 0,
        Immutable,
        Mutable,
    }

    private readonly Type type;
    private readonly ImmutableArray<GradientStop> immutableGradient;
    private readonly GradientStop[]? mutableGradient;

    public ReadOnlySpan<GradientStop> Gradient => type switch
    {
        Type.Constant => ReadOnlySpan<GradientStop>.Empty,
        Type.Immutable => immutableGradient.AsSpan(),
        Type.Mutable => mutableGradient,
        _ => throw new ArgumentOutOfRangeException(),
    };

    public GradientDefinition Definition { get; }

    private ShapeColor(GradientDefinition definition)
    {
        type = Type.Constant;
        Definition = definition;
    }

    private ShapeColor(ImmutableArray<GradientStop> gradient, GradientDefinition definition)
    {
        type = Type.Immutable;
        immutableGradient = gradient;
        Definition = definition;
    }

    private ShapeColor(GradientStop[] gradient, GradientDefinition definition)
    {
        type = Type.Mutable;
        mutableGradient = gradient;
        Definition = definition;
    }

    public bool IsNone => Definition.IsNone;

    public static implicit operator ShapeColor(Color color) => From(GradientDefinition.Constant(color));

    public static implicit operator ShapeColor(GradientDefinition.SingleColor color) => new(color);

    public static ShapeColor From(GradientDefinition.SingleColor parameters) => new(parameters);

    public static ShapeColor From(ImmutableArray<GradientStop> gradient, GradientDefinition parameters)
        => new(gradient, parameters);

    public static ShapeColor FromMutable(GradientStop[] gradient, GradientDefinition parameters)
        => new(gradient, parameters);

    public override string ToString()
        => $"{Definition.Type} with {(type == Type.Constant ? "NULL" : Gradient.Length)} stops";
}

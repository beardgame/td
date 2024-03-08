using System.Collections.Immutable;
using Bearded.Graphics;

namespace Bearded.TD.UI.Shapes;

readonly struct ShapeColor
{
    public ImmutableArray<GradientStop> Gradient { get; }
    public GradientDefinition Definition { get; }

    private ShapeColor(GradientDefinition definition)
        : this(ImmutableArray<GradientStop>.Empty, definition) { }

    private ShapeColor(ImmutableArray<GradientStop> gradient, GradientDefinition definition)
    {
        Gradient = gradient;
        Definition = definition;
    }

    public bool IsNone => Definition.IsNone;

    public static implicit operator ShapeColor(Color color) => From(GradientDefinition.Constant(color));

    public static implicit operator ShapeColor(GradientDefinition.SingleColor color) => new(color);

    public static ShapeColor From(GradientDefinition.SingleColor parameters) => new(parameters);

    public static ShapeColor From(ImmutableArray<GradientStop> gradient, GradientDefinition parameters)
        => new(gradient, parameters);
}

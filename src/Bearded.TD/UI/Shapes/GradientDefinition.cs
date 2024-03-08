using Bearded.Graphics;

namespace Bearded.TD.UI.Shapes;

readonly struct GradientDefinition
{
    public GradientType Type { get; }
    public Color Color { get; private init; }
    public AnchorPoint Point1 { get; private init; }
    public AnchorPoint Point2 { get; private init; }
    public float Radius { get; private init; }

    private GradientDefinition(GradientType type)
    {
        Type = type;
    }

    public bool IsNone => Type == GradientType.None;

    public static SingleColor Constant(Color color)
        => new(GradientTypeSingleColor.Constant, color);

    public static SingleColor SimpleGlow(Color color)
        => new(GradientTypeSingleColor.SimpleGlow, color);


    public static GradientDefinition Linear(AnchorPoint start, AnchorPoint end)
        => new(GradientType.Linear) { Point1 = start, Point2 = end };

    public static GradientDefinition Radial(AnchorPoint center, float radius)
        => new(GradientType.Radial) { Point1 = center, Radius = radius };

    public static GradientDefinition AlongEdgeNormal()
        => new(GradientType.AlongEdgeNormal);


    public readonly struct SingleColor(GradientTypeSingleColor type, Color color)
    {
        public GradientDefinition Definition { get; } = new((GradientType)type) { Color = color };

        public static implicit operator GradientDefinition(SingleColor definition) => definition.Definition;
    }
}

using System;
using Bearded.Graphics;
using Bearded.Utilities.Geometry;

namespace Bearded.TD.UI.Shapes;

[Flags]
enum GradientFlags : uint
{
    Default = 0,
    GlowFade = 1,
    Dither = 2,
}

readonly struct GradientDefinition
{
    public GradientType Type { get; }
    public Color Color { get; private init; }
    public AnchorPoint Point1 { get; private init; }
    public AnchorPoint Point2 { get; private init; }
    public float Radius { get; private init; }
    public Direction2 StartAngle { get; private init; }
    public Angle Length { get; private init; }
    public GradientFlags Flags { get; }

    private GradientDefinition(GradientType type, GradientFlags flags)
    {
        Type = type;
        Flags = flags;
    }

    public bool IsNone => Type == GradientType.None;

    public GradientDefinition WithFlags(GradientFlags flags)
        => new(Type, flags)
        {
            Color = Color,
            Point1 = Point1,
            Point2 = Point2,
            Radius = Radius,
            StartAngle = StartAngle,
            Length = Length,
        };

    public static SingleColor Constant(Color color)
        => new(GradientTypeSingleColor.Constant, color, GradientFlags.Default);

    public static SingleColor SimpleGlow(Color color)
        => new(GradientTypeSingleColor.Constant, color, GradientFlags.GlowFade | GradientFlags.Dither);

    public static GradientDefinition Linear(AnchorPoint start, AnchorPoint end)
        => new(GradientType.Linear, GradientFlags.Dither) { Point1 = start, Point2 = end };

    public static GradientDefinition Radial(AnchorPoint center, float radius)
        => new(GradientType.RadialWithRadius, GradientFlags.Dither) { Point1 = center, Radius = radius };

    public static GradientDefinition Radial(AnchorPoint center, AnchorPoint pointOnEdge)
        => new(GradientType.RadialToPoint, GradientFlags.Dither) { Point1 = center, Point2 = pointOnEdge };

    public static GradientDefinition AlongEdgeNormal()
        => new(GradientType.AlongEdgeNormal, GradientFlags.Dither);

    public static GradientDefinition ArcAroundPoint(AnchorPoint center, Direction2 start, Angle length)
        => new(GradientType.ArcAroundPoint, GradientFlags.Dither)
            { Point1 = center, StartAngle = start, Length = length };


    public readonly struct SingleColor(GradientTypeSingleColor type, Color color, GradientFlags flags)
    {
        public GradientDefinition Definition { get; } = new((GradientType)type, flags) { Color = color };

        public static implicit operator GradientDefinition(SingleColor definition) => definition.Definition;
    }

    public override string ToString()
    {
        return Type switch
        {
            GradientType.None => "{Type}",
            GradientType.Constant => $"{Type}({Color}, {Flags})",
            GradientType.Linear => $"{Type}({Point1} -> {Point2}, {Flags})",
            GradientType.RadialWithRadius => $"{Type}({Point1} -> {Radius}, {Flags})",
            GradientType.RadialToPoint => $"{Type}({Point1} -> {Point2}, {Flags})",
            GradientType.AlongEdgeNormal => $"{Type}",
            GradientType.ArcAroundPoint => $"{Type}({Point1}, {StartAngle} + {Length}, {Flags})",
            _ => $"{Type}(?)",
        };
    }
}

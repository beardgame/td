using System.Runtime.CompilerServices;
using Bearded.Graphics;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Shapes;

readonly struct GradientDefinition
{
    public readonly GradientType Type;
    public readonly Vector4 Parameters;

    private GradientDefinition(GradientType type, Vector4 parameters)
    {
        Type = type;
        Parameters = parameters;
    }

    public bool IsNone => Type == GradientType.None;

    public static SingleColor Constant(Color color)
        => new(GradientTypeSingleColor.Constant, (Unsafe.BitCast<Color, float>(color), 0, 0, 0));

    public static SingleColor SimpleGlow(Color color)
        => new(GradientTypeSingleColor.SimpleGlow, (Unsafe.BitCast<Color, float>(color), 0, 0, 0));


    public static GradientDefinition Linear(Vector2 start, Vector2 end)
        => new(GradientType.Linear, (start.X, start.Y, end.X, end.Y));

    public static GradientDefinition Radial(Vector2 center, float radius)
        => new(GradientType.Radial, (center.X, center.Y, radius, 0));

    public static GradientDefinition AlongEdgeNormal()
        => new(GradientType.AlongEdgeNormal, default);


    public readonly struct SingleColor(GradientTypeSingleColor type, Vector4 parameters)
    {
        public readonly GradientDefinition Definition = new((GradientType)type, parameters);

        public static implicit operator GradientDefinition(SingleColor definition) => definition.Definition;
    }
}

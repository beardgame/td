using Bearded.Graphics;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.UI.Gradients;

readonly struct GradientId(uint value)
{
    public readonly uint Value = value;
}

enum GradientType : byte
{
    Constant = 0,
    Linear = 1,
    Radial = 2,
    AlongEdgeNormal = 3,
}

readonly struct GradientParameters
{
    private readonly uint type1gradientIndex3;
    private readonly Vector4 parameters;

    private GradientParameters(GradientType type, GradientId gradientId, Vector4 parameters)
    {
        type1gradientIndex3 = (uint) type | (gradientId.Value << 8);
        this.parameters = parameters;
    }

    public static GradientParameters Constant(Color color)
        => new(GradientType.Constant, default, color.AsRGBAVector);

    public static GradientParameters Linear(GradientId gradientId, Vector2 start, Vector2 end)
        => new(GradientType.Linear, gradientId, (start.X, start.Y, end.X, end.Y));

    public static GradientParameters Radial(GradientId gradientId, Vector2 center, float radius)
        => new(GradientType.Radial, gradientId, (center.X, center.Y, radius, 0));

    public static GradientParameters AlongEdgeNormal(GradientId gradientId)
        => new(GradientType.AlongEdgeNormal, gradientId, default);
}

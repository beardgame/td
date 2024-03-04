using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.UI.Gradients;

readonly struct GradientId(uint value)
{
    public readonly uint Value = value < 0xFFFFFF ? value : throw new ArgumentOutOfRangeException(nameof(value));
}

enum GradientType : byte
{
    None = 0,

    // Single Color
    Constant = 1,
    SimpleGlow = 2,

    // Full Gradients
    Linear = 20,
    Radial = 21,
    AlongEdgeNormal = 22,
}

readonly struct GradientParameters
{
    private readonly uint type1gradientIndex3;
    private readonly Vector4 parameters;

    private GradientParameters(GradientType type, GradientId gradientId, Vector4 parameters)
    {
        type1gradientIndex3 = (uint)type | (gradientId.Value << 8);
        this.parameters = parameters;
    }

    public static IEnumerable<VertexAttributeTemplate> VertexAttributeTemplates(string prefix) =>
    [
        MakeAttributeTemplate<uint>($"v_{prefix}GradientTypeIndex"),
        MakeAttributeTemplate<Vector4>($"v_{prefix}GradientParameters"),
    ];

    public bool IsNone => (GradientType)(type1gradientIndex3 & 0xFF) == GradientType.None;

    public static implicit operator GradientParameters(Color color) => Constant(color);

    public static GradientParameters Constant(Color color)
        => new(GradientType.Constant, default, (Unsafe.BitCast<Color, float>(color), 0, 0, 0));

    public static GradientParameters SimpleGlow(Color color)
        => new(GradientType.SimpleGlow, default, (Unsafe.BitCast<Color, float>(color), 0, 0, 0));

    public static GradientParameters Linear(GradientId gradientId, Vector2 start, Vector2 end)
        => new(GradientType.Linear, gradientId, (start.X, start.Y, end.X, end.Y));

    public static GradientParameters Radial(GradientId gradientId, Vector2 center, float radius)
        => new(GradientType.Radial, gradientId, (center.X, center.Y, radius, 0));

    public static GradientParameters AlongEdgeNormal(GradientId gradientId)
        => new(GradientType.AlongEdgeNormal, gradientId, default);
}

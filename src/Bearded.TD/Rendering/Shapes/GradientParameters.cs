using System.Collections.Generic;
using Bearded.Graphics.Vertices;
using Bearded.TD.UI.Shapes;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Shapes;

readonly struct GradientParameters
{
    private readonly uint type1gradientIndex3;
    private readonly Vector4 parameters;

    // ReSharper disable once ConvertToPrimaryConstructor - easier to ensure correct binary format this way
    public GradientParameters(GradientType type, GradientId gradientId, Vector4 parameters)
    {
        type1gradientIndex3 = (uint)type | (gradientId.Value << 8);
        this.parameters = parameters;
    }

    public static IEnumerable<VertexAttributeTemplate> VertexAttributeTemplates(string prefix) =>
    [
        VertexData.MakeAttributeTemplate<uint>($"v_{prefix}GradientTypeIndex"),
        VertexData.MakeAttributeTemplate<Vector4>($"v_{prefix}GradientParameters"),
    ];

    public static implicit operator GradientParameters(GradientDefinition.SingleColor definition)
        => definition.Definition.ToGradientParameters(GradientId.None);
}

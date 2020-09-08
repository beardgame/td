using System.Runtime.InteropServices;
using amulware.Graphics.Vertices;
using OpenToolkit.Mathematics;
using static amulware.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Deferred
{
    [StructLayout(LayoutKind.Sequential)]
    readonly struct FluidVertex : IVertexData
    {
        // ReSharper disable NotAccessedField.Local
        private readonly Vector3 position;
        private readonly Vector3 normal;
        private readonly Vector2 flow;

        public FluidVertex(Vector3 position, Vector3 normal, Vector2 flow)
        {
            this.position = position;
            this.normal = normal;
            this.flow = flow;
        }

        private static readonly VertexAttribute[] vertexAttributes = MakeAttributeArray(
            MakeAttributeTemplate<Vector3>("vertexPosition"),
            MakeAttributeTemplate<Vector3>("vertexNormal"),
            MakeAttributeTemplate<Vector2>("vertexFlow")
        );

        VertexAttribute[] IVertexData.VertexAttributes => vertexAttributes;
    }
}

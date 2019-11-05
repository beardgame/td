using amulware.Graphics;
using OpenTK;
using static amulware.Graphics.VertexData;

namespace Bearded.TD.Rendering.Deferred
{
    struct FluidVertex : IVertexData
    {
        // ReSharper disable NotAccessedField.Local
        private Vector3 position;
        private Vector3 normal;
        private Vector2 flow;

        public FluidVertex(Vector3 position, Vector3 normal, Vector2 flow)
        {
            this.position = position;
            this.normal = normal;
            this.flow = flow;
        }

        public VertexAttribute[] VertexAttributes()
            => MakeAttributeArray(
                MakeAttributeTemplate<Vector3>("vertexPosition"),
                MakeAttributeTemplate<Vector3>("vertexNormal"),
                MakeAttributeTemplate<Vector2>("vertexFlow")
            );
    }
}

using System;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Vertices;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Loading
{
    // TODO: compare to ISprite interface and see what makes sense to have
    sealed class DrawableSprite<TVertex, TVertexData>
        where TVertex : struct, IVertexData
    {
        public delegate TVertex CreateSprite(Vector3 position, Vector2 uv, TVertexData data);

        private readonly IIndexedTrianglesMeshBuilder<TVertex, ushort> meshBuilder;
        private readonly CreateSprite createVertex;
        private readonly UVRectangle uv;

        public DrawableSprite(
            IIndexedTrianglesMeshBuilder<TVertex, ushort> meshBuilder,
            CreateSprite createVertex,
            UVRectangle uv)
        {
            this.meshBuilder = meshBuilder;
            this.createVertex = createVertex;
            this.uv = uv;
        }

        public void Draw(Vector3 center, float width, float height, float angle, TVertexData data)
        {
            var halfWidth = width * 0.5f;
            var halfHeight = height * 0.5f;

            var unitX = new Vector2((float) Math.Cos(angle), (float) Math.Sin(angle)) * halfWidth;
            var unitY = new Vector2(-unitX.Y, unitX.X) * halfHeight;

            Draw(center, unitX, unitY, data);
        }

        public void Draw(Vector3 center, Vector2 radiusX, Vector2 radiusY, TVertexData data)
        {
            var v0 = -radiusX + radiusY;
            var v1 = radiusX + radiusY;

            var v2 = -v0;
            var v3 = -v1;

            v0 += center.Xy;
            v1 += center.Xy;
            v2 += center.Xy;
            v3 += center.Xy;

            var z = center.Z;

            meshBuilder.AddQuad(
                createVertex(v0.WithZ(z), uv.TopLeft, data),
                createVertex(v1.WithZ(z), uv.TopRight, data),
                createVertex(v2.WithZ(z), uv.BottomRight, data),
                createVertex(v3.WithZ(z), uv.BottomLeft, data)
            );
        }
    }
}

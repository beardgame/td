using System;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Loading;

sealed class DrawableSprite<TVertex, TVertexData> : IDrawableSprite<TVertex, TVertexData>
    where TVertex : struct, IVertexData
{
    public delegate TVertex CreateSprite(Vector3 position, Vector2 uv, TVertexData data);

    private readonly IIndexedTrianglesMeshBuilder<TVertex, ushort> meshBuilder;
    private readonly CreateSprite createVertex;
    private readonly UVRectangle uv;
    private readonly Vector2 baseSize;

    public DrawableSprite(
        IIndexedTrianglesMeshBuilder<TVertex, ushort> meshBuilder,
        CreateSprite createVertex,
        SpriteParameters spriteParameters)
    {
        this.meshBuilder = meshBuilder;
        this.createVertex = createVertex;
        uv = spriteParameters.UV;
        baseSize = spriteParameters.BaseSize;
    }

    public void Draw(Vector3 center, float scale, float angle, TVertexData data)
    {
        ((IDrawableSprite<TVertexData>)this).Draw(
            center, baseSize.X * scale, baseSize.Y * scale, angle, data
        );
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

        ((IDrawableSprite<TVertexData>) this).DrawQuad(
            v0.WithZ(z), v1.WithZ(z), v2.WithZ(z), v3.WithZ(z), data
        );
    }

    public void DrawQuad(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft,
        TVertexData topLeftData, TVertexData topRightData, TVertexData bottomRightData, TVertexData bottomLeftData)
    {
        meshBuilder.AddQuad(
            createVertex(topLeft, uv.TopLeft, topLeftData),
            createVertex(topRight, uv.TopRight, topRightData),
            createVertex(bottomRight, uv.BottomRight, bottomRightData),
            createVertex(bottomLeft, uv.BottomLeft, bottomLeftData)
        );
    }

    public void DrawQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
        TVertexData data0, TVertexData data1, TVertexData data2, TVertexData data3)
    {
        meshBuilder.AddQuad(
            createVertex(p0, uv.Transform(uv0), data0),
            createVertex(p1, uv.Transform(uv1), data1),
            createVertex(p2, uv.Transform(uv2), data2),
            createVertex(p3, uv.Transform(uv3), data3)
        );
    }

    public void DrawIndexedVertices(
        int vertexCount, int indexCount, out Span<TVertex> vertices, out Span<ushort> indices,
        out ushort indexOffset, out UVRectangle uvs)
    {
        meshBuilder.Add(vertexCount, indexCount, out vertices, out indices, out indexOffset);
        uvs = uv;
    }
}

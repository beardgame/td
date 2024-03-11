using System;
using Bearded.TD.Rendering.Loading;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering;

interface IDrawableSprite<TVertex, in TVertexData> : IDrawableSprite<TVertexData>
{
    void DrawIndexedVertices(
        int vertexCount, int indexCount,
        out Span<TVertex> vertices, out Span<ushort> indices, out ushort indexOffset, out UVRectangle uvs);
}

interface IDrawableSprite<in TVertexData>
{
    void Draw(SpriteLayout layout, TVertexData data);

    void DrawQuad(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft, TVertexData data);

    void DrawQuad(
        Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
        Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
        TVertexData data0, TVertexData data1, TVertexData data2, TVertexData data3);
}

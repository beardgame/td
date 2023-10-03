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
    void Draw(Vector3 center, float scale, TVertexData data)
        => Draw(center, scale, 0, data);
    void Draw(Vector3 center, float scale, float angle, TVertexData data);

    void Draw(Vector3 center, float width, float height, float angle, TVertexData data)
    {
        var halfWidth = width * 0.5f;
        var halfHeight = height * 0.5f;

        var unitX = new Vector2((float) Math.Cos(angle), (float) Math.Sin(angle));
        var unitY = new Vector2(-unitX.Y, unitX.X);

        Draw(center, unitX * halfWidth, unitY * halfHeight, data);
    }

    void Draw(Vector3 center, Vector2 radiusX, Vector2 radiusY, TVertexData data);

    void DrawQuad(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft, TVertexData data)
        => DrawQuad(topLeft, topRight, bottomRight, bottomLeft, data, data, data, data);

    void DrawQuad(
        Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft,
        TVertexData topLeftData, TVertexData topRightData, TVertexData bottomRightData, TVertexData bottomLeftData);

    void DrawQuad(
        Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
        Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
        TVertexData data)
    {
        DrawQuad(p0, p1, p2, p3, uv0, uv1, uv2, uv3, data, data, data, data);
    }

    void DrawQuad(
        Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
        Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
        TVertexData data0, TVertexData data1, TVertexData data2, TVertexData data3);
}

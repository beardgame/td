using System;
using Bearded.Graphics.Vertices;
using Bearded.TD.Rendering.Loading;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Drawing;

interface IComponentDrawer
{
    void DrawSprite<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 position, float size, float angle, TVertexData data)
        where TVertex : struct, IVertexData;

    void DrawQuad<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite,
        Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft, TVertexData data)
        where TVertex : struct, IVertexData;

    void DrawQuad<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite,
        Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
        Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
        TVertexData data)
        where TVertex : struct, IVertexData;

    void DrawIndexedVertices<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite, int vertexCount, int indexCount,
        out Span<TVertex> vertices, out Span<ushort> indices, out ushort indexOffset, out UVRectangle uvs)
        where TVertex : struct, IVertexData;
}

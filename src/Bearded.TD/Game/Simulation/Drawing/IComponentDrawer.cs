using System;
using Bearded.Graphics.Vertices;
using Bearded.TD.Rendering.Loading;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Drawing;

interface IComponentDrawer
{
    void DrawSprite<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 position, float size, TVertexData data)
        where TVertex : struct, IVertexData
        => DrawSprite(sprite, position, size, Angle.Zero, data);

    void DrawSprite<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 position, float size, Direction2 direction, TVertexData data)
        where TVertex : struct, IVertexData
        => DrawSprite(sprite, position, size, direction - Direction2.Zero, data);

    void DrawSprite<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite, Vector3 position, float size, Angle angle, TVertexData data)
        where TVertex : struct, IVertexData;

    void DrawQuad<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite,
        Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft, TVertexData data)
        where TVertex : struct, IVertexData;

    void DrawIndexedVertices<TVertex, TVertexData>(
        SpriteDrawInfo<TVertex, TVertexData> sprite, int vertexCount, int indexCount,
        out Span<TVertex> vertices, out Span<ushort> indices, out ushort indexOffset, out UVRectangle uvs)
        where TVertex : struct, IVertexData;

    void DrawMesh(MeshDrawInfo mesh, Position3 position, Direction2 direction, Unit scale);
}

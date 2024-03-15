using System;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Vertices;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Loading;

sealed class DrawableSprite<TVertex, TVertexData>(
    IIndexedTrianglesMeshBuilder<TVertex, ushort> meshBuilder,
    CreateVertex<TVertex, TVertexData> createVertex,
    SpriteParameters spriteParameters)
        : IDrawableSprite<TVertex, TVertexData>
        where TVertex : struct, IVertexData
{
    private readonly UVRectangle uv = spriteParameters.UV;
    private readonly Vector2 baseSize = spriteParameters.BaseSize;

    public void Draw(SpriteLayout layout, TVertexData data)
    {
        var frame = layout.Frame;
        var size = layout.Size switch
        {
            SpriteSize.FrameAgnostic => baseSize,
            SpriteSize.StretchToFrame => (frame.Width, frame.Height),
            SpriteSize.ContainInFrame => baseSize * Math.Min(frame.Width / baseSize.X, frame.Height / baseSize.Y),
            SpriteSize.CoverFrame => baseSize * Math.Max(frame.Width / baseSize.X, frame.Height / baseSize.Y),
            _ => throw new ArgumentOutOfRangeException(nameof(layout.Size)),
        };

        var frameAnchor = frame.TopLeft + new Vector2(frame.Width, frame.Height) * layout.FrameAlign;
        var spriteAnchor = size * (layout.SpriteAlign - new Vector2(0.5f));
        var center = frameAnchor - spriteAnchor;

        size *= layout.Scale;

        draw(center.WithZ(layout.Z), size.X, size.Y, layout.Angle, data);
    }

    private void draw(Vector3 center, float width, float height, Angle angle, TVertexData data)
    {
        var halfWidth = width * 0.5f;
        var halfHeight = height * 0.5f;

        var radians = angle.Radians;
        var unitX = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
        var unitY = new Vector2(-unitX.Y, unitX.X);

        Draw(center, unitX * halfWidth, unitY * halfHeight, data);
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

        DrawQuad(v0.WithZ(z), v1.WithZ(z), v2.WithZ(z), v3.WithZ(z), data);
    }

    public void DrawQuad(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft, TVertexData data)
    {
        meshBuilder.AddQuad(
            createVertex(topLeft, uv.TopLeft, data),
            createVertex(topRight, uv.TopRight, data),
            createVertex(bottomRight, uv.BottomRight, data),
            createVertex(bottomLeft, uv.BottomLeft, data)
        );
    }

    public void DrawQuad(
        Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
        Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
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

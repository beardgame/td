using System;
using Bearded.Graphics;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Shapes;

namespace Bearded.TD.Rendering.Shapes;

interface IShapeDrawer : IShapeDrawer2<Color>;

sealed partial class ShapeDrawer : IShapeDrawer
{
    public void FillRectangle(
        float x, float y, float z, float w, float h, Color parameters)
    {
        meshBuilder.AddQuad(
            new(new(x, y, z), parameters),
            new(new(x + w, y, z), parameters),
            new(new(x + w, y + h, z), parameters),
            new(new(x, y + h, z), parameters)
        );
    }

    public void DrawRectangle(
        float x, float y, float z, float w, float h, float lineWidth, Color parameters)
    {
        meshBuilder.Add(8, 24, out var vertices, out var indices, out var indexOffset);

        // outer
        vertices[0] = new(new(x, y, z), parameters);
        vertices[1] = new(new(x + w, y, z), parameters);
        vertices[2] = new(new(x + w, y + h, z), parameters);
        vertices[3] = new(new(x, y + h, z), parameters);

        // inner
        vertices[4] = new(new(x + lineWidth, y + lineWidth, z), parameters);
        vertices[5] = new(new(x + w - lineWidth, y + lineWidth, z), parameters);
        vertices[6] = new(new(x + w - lineWidth, y + h - lineWidth, z), parameters);
        vertices[7] = new(new(x + lineWidth, y + h - lineWidth, z), parameters);

        var indicesIndex = 0;
        for (var i = 0; i < 4; i++)
        {
            var outer1 = i;
            var outer2 = (i + 1) % 4;
            var inner1 = outer1 + 4;
            var inner2 = outer2 + 4;

            indices[indicesIndex++] = (ushort) (indexOffset + outer1);
            indices[indicesIndex++] = (ushort) (indexOffset + outer2);
            indices[indicesIndex++] = (ushort) (indexOffset + inner2);

            indices[indicesIndex++] = (ushort) (indexOffset + outer1);
            indices[indicesIndex++] = (ushort) (indexOffset + inner2);
            indices[indicesIndex++] = (ushort) (indexOffset + inner1);
        }
    }

    public void FillOval(
        float centerX, float centerY, float centerZ, float radiusX, float radiusY, Color parameters, int edges)
    {
        FillRectangle(centerX - radiusX, centerY - radiusY, centerZ, radiusX * 2, radiusY * 2, parameters);
    }

    public void DrawOval(
        float centerX, float centerY, float centerZ, float radiusX, float radiusY, float lineWidth, Color parameters, int edges)
    {
        DrawRectangle(centerX - radiusX, centerY - radiusY, centerZ, radiusX * 2, radiusY * 2, lineWidth, parameters);
    }

    public void DrawLine(
        float x1, float y1, float z1, float x2, float y2, float z2, float lineWidth, Color parameters)
    {
        var vx = x2 - x1;
        var vy = y1 - y2; // switch order for correct normal direction
        var ilxy = 0.5f * lineWidth / (float)Math.Sqrt(vx * vx + vy * vy);
        var nx = vy * ilxy;
        var ny = vx * ilxy;
        meshBuilder.AddQuad(
            new(new(x1 + nx, y1 + ny, z1), parameters),
            new(new(x1 - nx, y1 - ny, z1), parameters),
            new(new(x2 - nx, y2 - ny, z2), parameters),
            new(new(x2 + nx, y2 + ny, z2), parameters)
        );
    }
}

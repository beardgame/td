﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Shapes;

[StructLayout(LayoutKind.Sequential)]
readonly struct ShapeVertex(
    Vector3 position, ShapeData shape, ShapeVertex.ShapeComponents components)
    : IVertexData
{
    private readonly Vector3 position = position;
    private readonly ShapeData shape = shape;
    private readonly ShapeComponents components = components;

    static ImmutableArray<VertexAttribute> IVertexData.VertexAttributes { get; }
        = MakeAttributeArray(
            [
                MakeAttributeTemplate<Vector3>("v_position"),
                ..ShapeData.VertexAttributeTemplates,
                ..ShapeComponents.VertexAttributeTemplates,
            ]
        );

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ShapeComponents(ShapeComponentId first, int count)
    {
        private readonly ShapeComponentId first = first;
        private readonly int count = count;

        public static IEnumerable<VertexAttributeTemplate> VertexAttributeTemplates =>
        [
            MakeAttributeTemplate<uint>("v_firstComponent"),
            MakeAttributeTemplate<int>("v_componentCount"),
        ];

        public static implicit operator ShapeComponents((ShapeComponentId first, int count) tuple)
            => new(tuple.first, tuple.count);

        public static implicit operator ShapeComponents(ShapeComponentIds ids)
            => new(ids.First, ids.Count);
    }
}

enum ShapeType
{
    Fill = 0,
    LinePointToPoint = 1,
    CirclePointRadius = 2,
    RectangleCornerSize = 3,
    HexagonPointRadius = 4,
}

[StructLayout(LayoutKind.Sequential)]
readonly struct ShapeData
{
    private readonly ShapeType type;
    private readonly Vector4 data;
    private readonly Vector3 data4;

    private ShapeData(ShapeType type, float d0, float d1, float d2, float d3, float d4, float d5, float d6)
    {
        this.type = type;
        data = new Vector4(d0, d1, d2, d3);
        data4 = new Vector3(d4, d5, d6);
    }

    public static IEnumerable<VertexAttributeTemplate> VertexAttributeTemplates =>
    [
        MakeAttributeTemplate<int>("v_shapeType"),
        MakeAttributeTemplate<Vector4>("v_shapeData"),
        MakeAttributeTemplate<Vector3>("v_shapeData2"),
    ];

    public static ShapeData Fill() => default;

    public static ShapeData LinePointToPoint(Vector2 start, Vector2 end) =>
        new(ShapeType.LinePointToPoint, start.X, start.Y, end.X, end.Y, 0, 0, 0);

    public static ShapeData CirclePointRadius(Vector2 center, float radius) =>
        new(ShapeType.CirclePointRadius, center.X, center.Y, radius, 0, 0, 0, 0);

    public static ShapeData RectangleCornerSize(
        Vector2 topLeft, Vector2 size, float cornerRadius, float cornerSquircleness, float innerGlowSmoothness)
        => new(ShapeType.RectangleCornerSize, topLeft.X, topLeft.Y, size.X, size.Y, cornerRadius, cornerSquircleness, innerGlowSmoothness);

    public static ShapeData HexagonPointRadius(
        Vector2 center, float radius, float cornerRadius, float innerGlowRoundness, float centerRoundness) =>
        new(ShapeType.HexagonPointRadius, center.X, center.Y, radius, cornerRadius, innerGlowRoundness, centerRoundness, 0);
}

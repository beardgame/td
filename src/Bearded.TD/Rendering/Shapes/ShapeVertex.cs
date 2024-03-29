﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Shapes;

[StructLayout(LayoutKind.Sequential)]
readonly struct ShapeVertex(Vector3 position, ShapeGeometry geometry, ShapeGradients gradients)
    : IVertexData
{
    private readonly Vector3 position = position;
    private readonly ShapeGeometry geometry = geometry;
    private readonly ShapeGradients gradients = gradients;

    static ImmutableArray<VertexAttribute> IVertexData.VertexAttributes { get; }
        = MakeAttributeArray(
            [
                MakeAttributeTemplate<Vector3>("v_position"),
                ..ShapeGeometry.VertexAttributeTemplates,
                ..ShapeGradients.VertexAttributeTemplates,
            ]
        );
}

[StructLayout(LayoutKind.Sequential)]
readonly struct ShapeGradients(
    GradientParameters fill = default,
    GradientParameters edge = default,
    GradientParameters outerGlow = default,
    GradientParameters innerGlow = default)
{
    public readonly GradientParameters Fill = fill;
    public readonly GradientParameters Edge = edge;
    public readonly GradientParameters OuterGlow = outerGlow;
    public readonly GradientParameters InnerGlow = innerGlow;

    public static IEnumerable<VertexAttributeTemplate> VertexAttributeTemplates =>
    [
        ..GradientParameters.VertexAttributeTemplates("fill"),
        ..GradientParameters.VertexAttributeTemplates("edge"),
        ..GradientParameters.VertexAttributeTemplates("outerGlow"),
        ..GradientParameters.VertexAttributeTemplates("innerGlow"),
    ];
}

[StructLayout(LayoutKind.Sequential)]
readonly struct ShapeGeometry
{
    // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
    private readonly int type;
    private readonly ShapeData data;
    private readonly EdgeData edge;

    private ShapeGeometry(ShapeType type, ShapeData data, EdgeData edge)
    {
        this.type = (int)type;
        this.data = data;
        this.edge = edge;
    }

    public static IEnumerable<VertexAttributeTemplate> VertexAttributeTemplates =>
    [
        MakeAttributeTemplate<int>("v_shapeType"),
        ..ShapeData.VertexAttributeTemplates,
        ..EdgeData.VertexAttributeTemplates,
    ];

    public static ShapeGeometry Fill() => new(ShapeType.Fill, ShapeData.Fill(), default);

    public static ShapeGeometry LinePointToPoint(Vector2 start, Vector2 end, EdgeData edge)
        => new(ShapeType.LinePointToPoint, ShapeData.LinePointToPoint(start, end), edge);

    public static ShapeGeometry CirclePointRadius(Vector2 center, float radius, EdgeData edge)
        => new(ShapeType.CirclePointRadius, ShapeData.CirclePointRadius(center, radius), edge);

    public static ShapeGeometry RectangleCornerSize(
        Vector2 topLeft, Vector2 size, float cornerRadius, EdgeData edge,
        float cornerSquircleness, float innerGlowSmoothness)
        => new(ShapeType.RectangleCornerSize, ShapeData
            .RectangleCornerSize(topLeft, size, cornerRadius, cornerSquircleness, innerGlowSmoothness), edge);

    public static ShapeGeometry HexagonPointRadius(
        Vector2 center, float radius, float cornerRadius, EdgeData edge,
        float innerGlowRoundness, float centerRoundness)
        => new(ShapeType.HexagonPointRadius, ShapeData
            .HexagonPointRadius(center, radius, cornerRadius, innerGlowRoundness, centerRoundness), edge);
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
    private readonly Vector4 data;
    private readonly Vector3 data4;

    private ShapeData(float d0, float d1, float d2, float d3, float d4, float d5, float d6)
    {
        data = new Vector4(d0, d1, d2, d3);
        data4 = new Vector3(d4, d5, d6);
    }

    public static IEnumerable<VertexAttributeTemplate> VertexAttributeTemplates =>
    [
        MakeAttributeTemplate<Vector4>("v_shapeData"),
        MakeAttributeTemplate<Vector3>("v_shapeData2"),
    ];

    public static ShapeData Fill() => default;

    public static ShapeData LinePointToPoint(Vector2 start, Vector2 end) =>
        new(start.X, start.Y, end.X, end.Y, 0, 0, 0);

    public static ShapeData CirclePointRadius(Vector2 center, float radius) =>
        new(center.X, center.Y, radius, 0, 0, 0, 0);

    public static ShapeData RectangleCornerSize(
        Vector2 topLeft, Vector2 size, float cornerRadius, float cornerSquircleness, float innerGlowSmoothness)
        => new(topLeft.X, topLeft.Y, size.X, size.Y, cornerRadius, cornerSquircleness, innerGlowSmoothness);

    public static ShapeData HexagonPointRadius(
        Vector2 center, float radius, float cornerRadius, float innerGlowRoundness, float centerRoundness) =>
        new(center.X, center.Y, radius, cornerRadius, innerGlowRoundness, centerRoundness, 0);
}

[StructLayout(LayoutKind.Sequential)]
readonly struct EdgeData(float outerWidth = 0, float innerWidth = 0, float outerGlow = 0, float innerGlow = 0)
{
    private readonly Vector4 data = new(outerWidth, innerWidth, outerGlow, innerGlow);

    public float OuterWidth => data.X;
    public float InnerWidth => data.Y;
    public float OuterGlow => data.Z;
    public float InnerGlow => data.W;

    public static IEnumerable<VertexAttributeTemplate> VertexAttributeTemplates =>
    [
        MakeAttributeTemplate<Vector4>("v_edgeData"),
    ];
}

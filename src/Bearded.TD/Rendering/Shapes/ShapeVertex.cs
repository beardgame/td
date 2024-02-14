﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Shapes;

[StructLayout(LayoutKind.Sequential)]
readonly struct ShapeVertex(Vector3 position, ShapeGeometry geometry, ShapeColors colors)
    : IVertexData
{
    private readonly Vector3 position = position;
    private readonly ShapeGeometry geometry = geometry;
    private readonly ShapeColors colors = colors;

    static ImmutableArray<VertexAttribute> IVertexData.VertexAttributes { get; }
        = MakeAttributeArray(
            [
                MakeAttributeTemplate<Vector3>("v_position"),
                ..ShapeGeometry.VertexAttributeTemplates,
                ..ShapeColors.VertexAttributeTemplates,
            ]
        );
}

[StructLayout(LayoutKind.Sequential)]
readonly struct ShapeColors(Color? fill = null, Color? edge = null, Color? outerGlow = null, Color? innerGlow = null)
{
    private readonly Color fill = fill ?? default;
    private readonly Color edge = edge ?? default;
    private readonly Color outerGlow = outerGlow ?? default;
    private readonly Color innerGlow = innerGlow ?? default;

    public bool HasFill => fill != default;

    public static IEnumerable<VertexAttributeTemplate> VertexAttributeTemplates =>
    [
        MakeAttributeTemplate("v_shapeColors", VertexAttribPointerType.UnsignedInt, 4, 16, VertexAttributeFormat.Integer),
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
}

enum ShapeType
{
    Fill = 0,
    LinePointToPoint = 1,
    CirclePointRadius = 2,
}

[StructLayout(LayoutKind.Sequential)]
readonly struct ShapeData
{
    private readonly Vector4 data;

    private ShapeData(float d0, float d1, float d2, float d3)
    {
        data = new Vector4(d0, d1, d2, d3);
    }

    public static IEnumerable<VertexAttributeTemplate> VertexAttributeTemplates =>
    [
        MakeAttributeTemplate<Vector4>("v_shapeData"),
    ];

    public static ShapeData Fill() => default;
    public static ShapeData LinePointToPoint(Vector2 start, Vector2 end) => new(start.X, start.Y, end.X, end.Y);
    public static ShapeData CirclePointRadius(Vector2 center, float radius) => new(center.X, center.Y, radius, 0);
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
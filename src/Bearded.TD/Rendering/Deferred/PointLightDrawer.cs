using System;
using Bearded.Graphics;
using Bearded.Graphics.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Deferred;

sealed class PointLightDrawer(PointLightMesh mesh)
{
    public void Draw(
        Vector3 center,
        float radius,
        Color color,
        float intensity = 1,
        bool drawShadow = true)
    {
        mesh.Add(new PointLightInstance(center, radius, color, intensity, drawShadow ? (byte)1 : (byte)0));
    }
}

sealed class PointLightMesh : IDisposable
{
    private readonly Buffer<PointLightVertex> vertices;
    private readonly Buffer<byte> indices;
    private readonly BufferStream<PointLightInstance> instances;

    private PointLightMesh(Buffer<PointLightVertex> vertices, Buffer<byte> indices, BufferStream<PointLightInstance> instances)
    {
        this.vertices = vertices;
        this.indices = indices;
        this.instances = instances;
    }

    public IRenderable ToRenderable()
    {
        return Renderable.Build(
            PrimitiveType.Triangles,
            b => b
                .With(vertices.AsVertexBuffer())
                .With(indices.AsIndexBuffer())
                .With(instances.AsVertexBuffer())
                .InstancedWith(() => instances.Count)
        );
    }

    public void Add(PointLightInstance instance)
    {
        instances.Add(instance);
    }

    public void Clear()
    {
        instances.Clear();
    }

    public void Dispose()
    {
        vertices.Dispose();
        indices.Dispose();
        instances.Buffer.Dispose();
    }

    public static PointLightMesh Create()
    {
        const float t = 1.61803398875f; // (1+sqrt(5))/2
        const float s = 1.90211303259f; // sqrt(1+t^2)
        const float scale = 1f / 0.794654472f; // 1f / sqrt((5 + 2 * sqrt(5)) / 15)

        const float a = scale * t / s;
        const float b = scale / s;

        Span<PointLightVertex> vertices = stackalloc PointLightVertex[12]
        {
            v(a, b, 0),
            v(-a, b, 0),
            v(a, -b, 0),
            v(-a, -b, 0),
            v(b, 0, a),
            v(b, 0, -a),
            v(-b, 0, a),
            v(-b, 0, -a),
            v(0, a, b),
            v(0, -a, b),
            v(0, a, -b),
            v(0, -a, -b),
        };

        Span<byte> indices = stackalloc byte[60]
        {
            0, 8, 4,
            0, 5, 10,
            2, 4, 9,
            2, 11, 5,
            1, 6, 8,
            1, 10, 7,
            3, 9, 6,
            3, 7, 11,
            0, 10, 8,
            1, 8, 10,
            2, 9, 11,
            3, 11, 9,
            4, 2, 0,
            5, 0, 2,
            6, 1, 3,
            7, 3, 1,
            8, 6, 4,
            9, 4, 6,
            10, 5, 7,
            11, 7, 5,
        };

        var vertexBuffer = new Buffer<PointLightVertex>();
        using (var target = vertexBuffer.Bind())
        {
            target.Upload(vertices);
        }

        var indexBuffer = new Buffer<byte>();
        using (var target = indexBuffer.Bind())
        {
            target.Upload(indices);
        }

        var instanceBuffer = new BufferStream<PointLightInstance>(new Buffer<PointLightInstance>());

        return new PointLightMesh(vertexBuffer, indexBuffer, instanceBuffer);

        static PointLightVertex v(float x, float y, float z) => new(new Vector3(x, y, z));
    }
}

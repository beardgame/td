using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using static Bearded.TD.Utilities.WorldMatrices;

namespace Bearded.TD.Rendering.Loading;

sealed class DrawableMesh : IDrawable
{
    private readonly Mesh mesh;
    private readonly BufferStream<MeshInstance> instances;

    private DrawableMesh(Mesh mesh, BufferStream<MeshInstance> instances)
    {
        this.mesh = mesh;
        this.instances = instances;
    }

    public void Add(Vector3 offset, Angle rotationY, float scale = 1.0f)
    {
        var worldMatrix = RotateY(rotationY).Then(Scale(scale)).Then(Translate(offset)).Matrix;
        Add(worldMatrix);
    }

    public void Add(Matrix4 worldMatrix)
    {
        Add(new MeshInstance(worldMatrix));
    }

    public void Add(MeshInstance instance)
    {
        instances.Add(instance);
    }

    public void Clear()
    {
        instances.Clear();
    }

    public void Dispose()
    {
        instances.Buffer.Dispose();
    }

    public IRenderer CreateRendererWithSettings(IEnumerable<IRenderSetting> settings)
    {
        var renderable = Renderable.Build(PrimitiveType.Triangles, b => b
            .With(mesh.VertexBuffer)
            .With(mesh.IndexBuffer)
            .With(instances.AsVertexBuffer())
            .InstancedWith(() => instances.Count));
        // TODO: upload material
        return Renderer.From(renderable, settings);
    }

    public static DrawableMesh ForMesh(Mesh mesh)
    {
        var instanceBuffer = new BufferStream<MeshInstance>(new Buffer<MeshInstance>());

        return new DrawableMesh(mesh, instanceBuffer);
    }
}

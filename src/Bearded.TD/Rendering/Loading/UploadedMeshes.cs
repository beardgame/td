using System;
using System.Collections.Immutable;
using Bearded.Graphics;
using Bearded.Graphics.Rendering;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Vertices;

namespace Bearded.TD.Rendering.Loading;

sealed class UploadedMeshes(ImmutableDictionary<string, Mesh> meshes) : IMeshesImplementation
{
    public IMesh GetMesh(string key) => meshes[key];

    public void Dispose()
    {
        foreach (var mesh in meshes.Values)
        {
            mesh.Dispose();
        }
    }
}

sealed class Mesh(Buffer<NormalUVVertex> vertices, Buffer<ushort> indices) : IDisposable, IDrawableTemplate, IMesh
{
    public IVertexBuffer VertexBuffer { get; } = vertices.AsVertexBuffer();
    public IIndexBuffer IndexBuffer { get; } = indices.AsIndexBuffer();

    public DrawableMesh AsDrawable(
        IDrawableRenderers drawableRenderers,
        DrawOrderGroup drawGroup,
        int drawGroupOrderKey,
        Shader shader)
    {
        return drawableRenderers.GetOrCreateDrawableFor(this, shader, drawGroup, drawGroupOrderKey, makeDrawable);
    }

    private DrawableMesh makeDrawable() => DrawableMesh.ForMesh(this);

    public void Dispose()
    {
        vertices.Dispose();
        indices.Dispose();
    }
}

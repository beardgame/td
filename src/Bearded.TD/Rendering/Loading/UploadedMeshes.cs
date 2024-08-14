using System;
using System.Collections.Immutable;
using Bearded.Graphics;
using Bearded.Graphics.Rendering;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Vertices;

namespace Bearded.TD.Rendering.Loading;

sealed class UploadedMeshes(ImmutableArray<Mesh> meshes) : IMeshesImplementation
{
    public void Dispose()
    {
        foreach (var mesh in meshes)
        {
            mesh.Dispose();
        }
    }
}

sealed class Mesh(Buffer<NormalUVVertex> vertices, Buffer<ushort> indices) : IDisposable
{
    public IVertexBuffer VertexBuffer { get; } = vertices.AsVertexBuffer();
    public IIndexBuffer IndexBuffer { get; } = indices.AsIndexBuffer();

    public void Dispose()
    {
        vertices.Dispose();
        indices.Dispose();
    }
}

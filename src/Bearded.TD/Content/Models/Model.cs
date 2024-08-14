using System;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Rendering.Vertices;

namespace Bearded.TD.Content.Models;

sealed class Model : IBlueprint, IDisposable
{
    public ModAwareId Id { get; }

    public Model(ModAwareId id)
    {
        Id = id;
    }

    public void Dispose()
    {
    }
}

sealed class Mesh(Buffer<NormalUVVertex> vertices, Buffer<ushort> indices) : IDisposable
{
    public void Dispose()
    {
        vertices.Dispose();
        indices.Dispose();
    }
}

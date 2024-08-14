using System;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;

namespace Bearded.TD.Content.Models;

sealed class Model : IBlueprint, IDisposable
{
    private readonly IMeshesImplementation meshes;

    public ModAwareId Id { get; }

    public Model(ModAwareId id, IMeshesImplementation meshes)
    {
        Id = id;
        this.meshes = meshes;
    }

    public void Dispose()
    {
        meshes.Dispose();
    }
}

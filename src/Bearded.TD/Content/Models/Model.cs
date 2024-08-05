using System;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;

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

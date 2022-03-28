using System;
using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.GameObjects;

static class ComponentGameObjectProperties
{
    public static Id<ComponentGameObject> FindId(this ComponentGameObject componentOwner)
    {
        if (!componentOwner.TryGetSingleComponent<IIdProvider>(out var idProvider))
        {
            throw new InvalidOperationException("Cannot get the ID of a component owner without ID.");
        }
        return idProvider.Id;
    }
}

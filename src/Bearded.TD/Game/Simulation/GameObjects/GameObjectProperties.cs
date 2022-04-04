using System;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.GameObjects;

static class GameObjectProperties
{
    public static Id<GameObject> FindId(this GameObject owner)
    {
        if (!owner.TryGetSingleComponent<IIdProvider>(out var idProvider))
        {
            throw new InvalidOperationException("Cannot get the ID of a component owner without ID.");
        }
        return idProvider.Id;
    }
}

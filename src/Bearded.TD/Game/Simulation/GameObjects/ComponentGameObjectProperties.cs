using System;
using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.GameObjects
{
    static class ComponentGameObjectProperties
    {
        public static Id<T> FindId<T>(this T componentOwner)
            where T : IComponentOwner<T>
        {
            if (!componentOwner.TryGetSingleComponent<IIdProvider<T>>(out var idProvider))
            {
                throw new InvalidOperationException("Cannot get the ID of a component owner without ID.");
            }
            return idProvider.Id;
        }
    }
}

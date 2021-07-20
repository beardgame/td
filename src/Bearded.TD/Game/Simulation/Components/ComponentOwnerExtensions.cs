using Bearded.Utilities;
using static Bearded.Utilities.Maybe;

namespace Bearded.TD.Game.Simulation.Components
{
    static class ComponentOwnerExtensions
    {
        public static Maybe<T> FindInComponentOwnerTree<T>(this IComponentOwner componentOwner)
        {
            if (componentOwner is T typeToFind)
                return Just(typeToFind);

            return componentOwner.Parent.SelectMany(FindInComponentOwnerTree<T>);
        }
    }
}

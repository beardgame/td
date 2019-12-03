using Bearded.Utilities;
using static Bearded.Utilities.Maybe;

namespace Bearded.TD.Game.Components
{
    static class ComponentOwnerExtensions
    {
        public static Maybe<T> FindInComponentOwnerTree<T>(this IComponentOwner componentOwner)
        {
            if (componentOwner is T damageOwner)
                return Just(damageOwner);

            return componentOwner.Parent.SelectMany(FindInComponentOwnerTree<T>);
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Bearded.TD.Game.Simulation.GameObjects;

static class ComponentOwnerExtensions
{
    public static bool TryGetSingleComponentInOwnerTree<T>(this IComponentOwner componentOwner,
        [NotNullWhen(true)] out T? component)
    {
        return componentOwner.TryGetSingleComponent(out component) ||
            (componentOwner.Parent?.TryGetSingleComponentInOwnerTree(out component) ?? false);
    }

    public static bool TryGetSingleComponent<T>(
        this IComponentOwner componentOwner, [NotNullWhen(true)] out T? component)
    {
        component = componentOwner.GetComponents<T>().SingleOrDefault();
        return !Equals(component, default(T));
    }
}
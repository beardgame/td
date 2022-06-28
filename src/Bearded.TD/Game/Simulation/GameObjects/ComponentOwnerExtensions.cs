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

    public static bool TryGetProperty<T>(this IComponentOwner componentOwner, [NotNullWhen(true)] out T? property)
    {
        if (componentOwner.TryGetSingleComponent<IProperty<T>>(out var propertyComp))
        {
            property = propertyComp.Value;
            return true;
        }

        property = default;
        return false;
    }

    public static bool HasTag(this IComponentOwner componentOwner, string tag)
    {
        return componentOwner.TryGetSingleComponent<GameObjectTags>(out var tags) && tags.HasTag(tag);
    }
}

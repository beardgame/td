using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Bearded.TD.Game.Simulation.GameObjects;

static class GameObjectExtensions
{
    public static bool TryGetSingleComponentInOwnerTree<T>(this GameObject obj, [NotNullWhen(true)] out T? component)
    {
        return obj.TryGetSingleComponent(out component) ||
            (obj.Parent?.TryGetSingleComponentInOwnerTree(out component) ?? false);
    }

    public static bool TryGetSingleComponent<T>(this GameObject obj, [NotNullWhen(true)] out T? component)
    {
        component = obj.GetComponents<T>().SingleOrDefault();
        return !Equals(component, default(T));
    }

    public static bool TryGetProperty<T>(this GameObject obj, [NotNullWhen(true)] out T? property)
    {
        if (obj.TryGetSingleComponent<IProperty<T>>(out var propertyComp))
        {
            property = propertyComp.Value;
            return true;
        }

        property = default;
        return false;
    }

    public static ImmutableHashSet<string> Tags(this GameObject obj)
    {
        return obj.TryGetSingleComponent<GameObjectTags>(out var tags)
            ? tags.Tags
            : ImmutableHashSet<string>.Empty;
    }
}

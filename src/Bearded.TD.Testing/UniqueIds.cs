using Bearded.TD.Content.Mods;
using Bearded.Utilities;

namespace Bearded.TD.Testing;

static class UniqueIds
{
    private static readonly IdManager idManager = new();

    public static Id<T> NextUniqueId<T>() => idManager.GetNext<T>();

    public static ExternalId<T> NextUniqueExternalId<T>(string prefix = "id") =>
        ExternalId<T>.FromLiteral($"{prefix}{idManager.GetNext<T>().Value}");
}

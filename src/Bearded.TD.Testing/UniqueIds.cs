using Bearded.TD.Content.Mods;
using Bearded.Utilities;

namespace Bearded.TD.Testing;

static class UniqueIds
{
    private static readonly IdManager idManager = new();

    public static Id<T> NextUniqueId<T>() => idManager.GetNext<T>();

    public static ExternalId<T> NextUniqueExternalId<T>(string prefix = "id") =>
        ExternalId<T>.FromLiteral(NextUniquePrefixedString<T>(prefix));

    public static ModAwareId NextUniqueModAwareId(string modName = "default", string prefix = "blueprint") =>
        ModAwareId.FromFullySpecified($"{modName}.{NextUniquePrefixedString<ModAwareId>(prefix)}");

    public static string NextUniquePrefixedString<TKey>(string prefix) => $"{prefix}{idManager.GetNext<TKey>().Value}";
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bearded.TD.Generators.Types;

sealed class TypeAliases : IEnumerable<TypeAlias>
{
    private readonly HashSet<string> usedShortNames = new();
    private readonly Dictionary<FullTypeName, ShortTypeName> aliases = [];

    public void AddRange(IEnumerable<TypeName> type)
    {
        foreach (var t in type)
        {
            Add(t);
        }
    }

    public void Add(TypeName typeName)
    {
        if (aliases.ContainsKey(typeName.FullName))
            return;

        var cleanName = Regex.Replace(typeName.ShortName.Name, "[.,<]", "_");
        cleanName = Regex.Replace(cleanName, "[> ]", "");

        var uniqueName = cleanName;
        var i = 1;
        while (!usedShortNames.Add(uniqueName))
        {
            uniqueName = $"{cleanName}_{++i}";
        }

        aliases[typeName.FullName] = new ShortTypeName(uniqueName);
    }

    public ShortTypeName this[TypeName typeName]
    {
        get
        {
            if (aliases.TryGetValue(typeName.FullName, out var alias))
            {
                return alias;
            }

            throw new InvalidOperationException($"No alias found for type {typeName.FullName}");
        }
    }

    public bool TryGetAliasFor(TypeName typeName, out ShortTypeName alias)
    {
        return aliases.TryGetValue(typeName.FullName, out alias);
    }

    public ShortTypeName GetOrAddAliasFor(TypeName typeName)
    {
        Add(typeName);
        return aliases[typeName.FullName];
    }

    public IEnumerator<TypeAlias> GetEnumerator()
    {
        return aliases.Select(kvp => new TypeAlias(kvp.Key, kvp.Value)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

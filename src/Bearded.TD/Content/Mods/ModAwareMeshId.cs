using System;
using System.IO;

namespace Bearded.TD.Content.Mods;

readonly record struct ModAwareMeshId(ModAwareId Model, string Id)
{
    public static ModAwareMeshId FromNameInMod(string name, ModMetadata mod)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Mesh id may not be null, empty or whitespace.", nameof(name));

        var components = name.Split('.');

        return components.Length switch
        {
            1 => throw new InvalidDataException($"Mesh id must contain at least one '.' ({name})"),
            2 => new ModAwareMeshId(new ModAwareId(mod.Id, components[0]), components[1]),
            _ => new ModAwareMeshId(new ModAwareId(components[0], components[1]), string.Join('.', components[2..]))
        };
    }

    public static ModAwareMeshId FromFullySpecified(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Mesh id may not be null, empty or whitespace.", nameof(name));

        var components = name.Split('.');
        if (components.Length != 3)
        {
            throw new InvalidDataException($"Mesh must contain exactly 2 . ({name})");
        }

        return new ModAwareMeshId(new ModAwareId(components[0], components[1]), components[2]);
    }

    public override string ToString() => $"{Model}.{Id}";
}

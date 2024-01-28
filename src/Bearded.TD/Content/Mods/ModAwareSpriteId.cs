using System.IO;

namespace Bearded.TD.Content.Mods;

readonly record struct ModAwareSpriteId(ModAwareId SpriteSet, string Id)
{
    public static ModAwareSpriteId FromNameInMod(string name, ModMetadata mod)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidDataException("Sprite id may not be null, empty or whitespace.");

        var components = name.Split('.');

        return components.Length switch
        {
            1 => throw new InvalidDataException($"Srite id must contain at least one '.' ({name})"),
            2 => new ModAwareSpriteId(new ModAwareId(mod.Id, components[0]), components[1]),
            3 => new ModAwareSpriteId(new ModAwareId(components[0], components[1]), components[2]),
            _ => throw new InvalidDataException($"Sprite id may not contain more than two '.' ({name})")
        };
    }

    public override string ToString()
        => $"{SpriteSet}.{Id}";
}

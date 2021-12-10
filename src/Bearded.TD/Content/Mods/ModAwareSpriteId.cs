using System.IO;

namespace Bearded.TD.Content.Mods;

struct ModAwareSpriteId
{
    public ModAwareId SpriteSet { get; }
    public string Id { get; }

    public ModAwareSpriteId(ModAwareId spriteSet, string id)
    {
        SpriteSet = spriteSet;
        Id = id;
    }

    public static ModAwareSpriteId FromNameInMod(string name, ModMetadata mod)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidDataException("Sprite id may not be null, empty or whitespace.");

        var components = name.Split('.');

        switch (components.Length)
        {
            case 1:
                throw new InvalidDataException($"Srite id must contain at least one . ({name})");
            case 2:
                return new ModAwareSpriteId(new ModAwareId(mod.Id, components[0]), components[1]);
            case 3:
                return new ModAwareSpriteId(new ModAwareId(components[0], components[1]), components[2]);
            default:
                throw new InvalidDataException($"Sprite id may not contain more than two . ({name})");
        }
    }

    public override string ToString()
        => $"{SpriteSet}.{Id}";
}
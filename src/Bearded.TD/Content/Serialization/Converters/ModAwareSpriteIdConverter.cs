using System.IO;
using Bearded.TD.Content.Mods;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class ModAwareSpriteIdConverter : JsonConverterBase<ModAwareSpriteId>
{
    private readonly ModMetadata? modContext;

    private ModAwareSpriteIdConverter(ModMetadata? meta)
    {
        modContext = meta;
    }

    protected override ModAwareSpriteId ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        var o = reader.Value;
        if (o is not string s)
        {
            throw new InvalidDataException($"Found unexpected value for mod aware sprite ID: {o}");
        }

        return modContext == null
            ? ModAwareSpriteId.FromFullySpecified(s)
            : ModAwareSpriteId.FromNameInMod(s, modContext);
    }

    protected override void WriteJson(JsonWriter writer, ModAwareSpriteId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public static ModAwareSpriteIdConverter WithinMod(ModMetadata meta) => new(meta);
}

using System.IO;
using Bearded.TD.Content.Mods;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class ModAwareIdConverter : JsonConverterBase<ModAwareId>
{
    private readonly ModMetadata? modContext;

    private ModAwareIdConverter(ModMetadata? meta)
    {
        modContext = meta;
    }

    protected override ModAwareId ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        var o = reader.Value;
        if (o is not string s)
        {
            throw new InvalidDataException($"Found unexpected value for mod aware ID: {o}");
        }
        return modContext == null ? ModAwareId.FromFullySpecified(s) : ModAwareId.FromNameInMod(s, modContext);
    }

    protected override void WriteJson(JsonWriter writer, ModAwareId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public static ModAwareIdConverter WithinMod(ModMetadata meta) => new(meta);

    public static ModAwareIdConverter Global() => new(null);
}

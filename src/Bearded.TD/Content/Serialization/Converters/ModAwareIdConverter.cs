using Bearded.TD.Content.Mods;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class ModAwareIdConverter : JsonConverterBase<ModAwareId>
{
    protected override ModAwareId ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        var o = reader.Value;
        return ModAwareId.FromFullySpecified((string) o);
    }

    protected override void WriteJson(JsonWriter writer, ModAwareId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
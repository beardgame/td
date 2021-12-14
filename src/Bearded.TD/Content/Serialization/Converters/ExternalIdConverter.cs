using Bearded.TD.Content.Mods;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class ExternalIdConverter<T> : JsonConverterBase<ExternalId<T>>
{
    protected override ExternalId<T> ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        var o = reader.Value;
        return ExternalId<T>.FromLiteral((string) o);
    }
}
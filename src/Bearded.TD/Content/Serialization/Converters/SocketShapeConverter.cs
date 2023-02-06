using Bearded.TD.Game.Simulation.Enemies;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class SocketShapeConverter : JsonConverterBase<SocketShape>
{
    protected override SocketShape ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        var o = reader.Value;
        return SocketShape.FromLiteral((string) o ?? string.Empty);
    }
}

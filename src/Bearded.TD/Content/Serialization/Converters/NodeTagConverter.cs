using Bearded.TD.Game.Generation.Semantic.Features;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class NodeTagConverter : JsonConverterBase<NodeTag>
    {
        protected override NodeTag ReadJson(JsonReader reader, JsonSerializer serializer)
        {
            var str = serializer.Deserialize<string>(reader);
            return new NodeTag(str);
        }
    }
}

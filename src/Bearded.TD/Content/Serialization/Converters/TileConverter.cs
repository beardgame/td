using System.Text.Json;
using Bearded.TD.Tiles;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class TileConverter : JsonConverterBase<Tile>
    {
        protected override Tile ReadJson(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var coords = JsonSerializer.Deserialize<int[]>(ref reader, options);

            if (coords == null || coords.Length != 2)
            {
                throw new JsonException("Tiles must have exactly two coordinates.");
            }

            return new Tile(coords[0], coords[1]);
        }
    }
}

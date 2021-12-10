using System.IO;
using Bearded.TD.Tiles;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class TileConverter : JsonConverterBase<Tile>
{
    protected override Tile ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        var coords = serializer.Deserialize<int[]>(reader);

        if (coords.Length != 2)
        {
            throw new InvalidDataException("Tiles must have exactly two coordinates.");
        }

        return new Tile(coords[0], coords[1]);
    }
}
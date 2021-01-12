using System.Text.Json;
using System.Text.Json.Serialization;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class PositionedFootprintConverter : JsonConverterBase<PositionedFootprint>
    {
        protected override PositionedFootprint ReadJson(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var jsonModel = JsonSerializer.Deserialize<JsonModel>(ref reader, options);
            var footprint = JsonSerializer.Deserialize<FootprintGroup>(jsonModel.Group);
            return footprint!.Positioned(jsonModel.Index, jsonModel.RootTile);
        }

        private readonly struct JsonModel
        {
            public string Group { get; }
            public Tile RootTile { get; }
            public int Index { get; }

            [UsedImplicitly]
            [JsonConstructor]
            public JsonModel(string group, Tile rootTile, int index = 0)
            {
                Group = group;
                RootTile = rootTile;
                Index = index;
            }
        }
    }
}

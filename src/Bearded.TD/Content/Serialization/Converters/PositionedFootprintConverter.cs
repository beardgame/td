using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class PositionedFootprintConverter : JsonConverterBase<PositionedFootprint>
    {
        private readonly IDependencyResolver<FootprintGroup> footprintDependencyResolver;

        public PositionedFootprintConverter(IDependencyResolver<FootprintGroup> footprintDependencyResolver)
        {
            this.footprintDependencyResolver = footprintDependencyResolver;
        }

        protected override PositionedFootprint ReadJson(JsonReader reader, JsonSerializer serializer)
        {
            var jsonModel = serializer.Deserialize<JsonModel>(reader);
            var footprint = footprintDependencyResolver.Resolve(jsonModel.Group);
            return footprint.Positioned(jsonModel.Index, jsonModel.RootTile);
        }

        private readonly struct JsonModel
        {
            public string Group { get; }
            public Tile RootTile { get; }
            public int Index { get; }

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

using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class PositionedFootprintConverter : JsonConverterBase<PositionedFootprint>
{
    private readonly IDependencyResolver<IFootprint> footprintDependencyResolver;

    public PositionedFootprintConverter(IDependencyResolver<IFootprint> footprintDependencyResolver)
    {
        this.footprintDependencyResolver = footprintDependencyResolver;
    }

    protected override PositionedFootprint ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        var jsonModel = serializer.Deserialize<JsonModel>(reader);
        var footprint = footprintDependencyResolver.Resolve(jsonModel.Footprint);
        return footprint.Positioned(jsonModel.RootTile, jsonModel.Orientation ?? Orientation.Default);
    }

    private readonly record struct JsonModel(string Footprint, Tile RootTile, Orientation? Orientation);
}

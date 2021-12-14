using Bearded.TD.Content.Mods;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation;
using Bearded.Utilities;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class FlattenedBlueprintConverter<TInterface, TJsonModel> : JsonConverterBase<TInterface>
    where TInterface : IBlueprint
    where TJsonModel : IConvertsTo<TInterface, Void>
{
    private readonly ModMetadata modMetadata;

    public FlattenedBlueprintConverter(ModMetadata modMetadata)
    {
        this.modMetadata = modMetadata;
    }

    protected override TInterface ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        var jsonModel = serializer.Deserialize<TJsonModel>(reader);
        return jsonModel.ToGameModel(modMetadata, default);
    }
}
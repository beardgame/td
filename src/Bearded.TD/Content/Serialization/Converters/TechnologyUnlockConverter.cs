using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class TechnologyUnlockConverter : JsonConverter<ITechnologyUnlock>
    {
        private readonly IDependencyResolver<IBuildingBlueprint> buildingResolver;
        private readonly IDependencyResolver<IUpgradeBlueprint> upgradeResolver;

        public TechnologyUnlockConverter(
            IDependencyResolver<IBuildingBlueprint> buildingResolver,
            IDependencyResolver<IUpgradeBlueprint> upgradeResolver)
        {
            this.buildingResolver = buildingResolver;
            this.upgradeResolver = upgradeResolver;
        }

        public override void Write(Utf8JsonWriter writer, ITechnologyUnlock value, JsonSerializerOptions options) =>
            throw new NotImplementedException();

        public override ITechnologyUnlock Read(
            ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions options)
        {
            var jsonModel = JsonSerializer.Deserialize<TechnologyBlueprint.TechnologyUnlock>(ref reader, options);
            if (jsonModel == null)
            {
                throw new JsonException("Could not parse technology unlock.");
            }
            return jsonModel.ToGameModel(buildingResolver, upgradeResolver);
        }
    }
}

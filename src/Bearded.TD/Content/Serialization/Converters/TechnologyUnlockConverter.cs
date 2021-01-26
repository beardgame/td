using System;
using Bearded.TD.Content.Mods;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class TechnologyUnlockConverter : JsonConverter
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

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotSupportedException();

        public override object ReadJson(
            JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonModel = serializer.Deserialize<TechnologyBlueprint.TechnologyUnlock>(reader);
            return jsonModel.ToGameModel(buildingResolver, upgradeResolver);
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(ITechnologyUnlock);
    }
}

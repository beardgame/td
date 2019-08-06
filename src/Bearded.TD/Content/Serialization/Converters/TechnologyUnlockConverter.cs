using System;
using System.IO;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Technologies;
using Bearded.TD.Game.Upgrades;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class TechnologyUnlockConverter : JsonConverterBase<ITechnologyUnlock>
    {
        private enum TechnologyUnlockType
        {
            Unknown = 0,
            Building = 1,
            Upgrade = 2,
        }

        protected override ITechnologyUnlock ReadJson(JsonReader reader, JsonSerializer serializer)
        {
            var json = JObject.Load(reader);

            var type = json
                .GetValue("type", StringComparison.OrdinalIgnoreCase)
                ?.ToObject<TechnologyUnlockType>(serializer) ?? TechnologyUnlockType.Unknown;
            var blueprintJson = json.GetValue("blueprint", StringComparison.OrdinalIgnoreCase);

            if (blueprintJson == null)
            {
                throw new InvalidDataException("Technology unlock must specify a blueprint name.");
            }

            switch (type)
            {
                case TechnologyUnlockType.Building:
                    return new BuildingUnlock(blueprintJson.ToObject<IBuildingBlueprint>(serializer));
                case TechnologyUnlockType.Upgrade:
                    return new UpgradeUnlock(blueprintJson.ToObject<IUpgradeBlueprint>(serializer));
                default:
                    throw new InvalidDataException("Technology unlock must have a valid type.");
            }
        }
    }
}

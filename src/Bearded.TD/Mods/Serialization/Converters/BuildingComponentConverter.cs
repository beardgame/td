using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Mods.Models;
using Bearded.TD.Mods.Serialization.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bearded.TD.Mods.Serialization.Converters
{
    static class BuildingComponentConverterFactory
    {
        public static BuildingComponentConverter Make()
        {
            return new BuildingComponentConverter(new Dictionary<string, Type>
            {
                { "workerHub", typeof(WorkerHubParameters) },
                { "incomeOverTime", typeof(IncomeOverTimeParameters) },
                { "turret", typeof(TurretParameters) },
            });
        }
    }

    sealed class BuildingComponentConverter : JsonConverterBase<IBuildingComponent>
    {
        private readonly Dictionary<string, Type> componentTypes;

        public BuildingComponentConverter(Dictionary<string, Type> componentParameters)
        {
            Type GenericComponent(Type parameter)
                => typeof(BuildingComponent<>).MakeGenericType(parameter);

            componentTypes = componentParameters
                .ToDictionary(t => t.Key, t => GenericComponent(t.Value));
        }

        protected override IBuildingComponent ReadJson(JsonReader reader, JsonSerializer serializer)
        {
            var json = JObject.Load(reader);

            var id = json
                .GetValue("id", StringComparison.OrdinalIgnoreCase)
                ?.Value<string>();

            if (id == null)
                throw new InvalidDataException("Component must have an id.");
            
            if (!componentTypes.TryGetValue(id, out var componentType))
                throw new InvalidDataException($"Unknown component id '{id}'.");

            var component = Activator.CreateInstance(componentType);
            serializer.Populate(json.CreateReader(), component);
            return (IBuildingComponent) component;
        }
    }
}

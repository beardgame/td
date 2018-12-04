using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Mods.Serialization.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bearded.TD.Mods.Serialization.Converters
{
    static class BuildingComponentConverterFactory
    {
        public static BuildingComponentConverter<IBuildingComponent> Make()
        {
            return new BuildingComponentConverter<IBuildingComponent>(
                typeof(BuildingComponent<>), ComponentFactories.ParameterTypesForComponentsById);
        }
    }

    sealed class BuildingComponentConverter<TComponentInterface> : JsonConverterBase<TComponentInterface>
    {
        private readonly Dictionary<string, Type> componentTypes;

        public BuildingComponentConverter(Type genericComponentType, IDictionary<string, Type> componentParameterTypes)
        {
            Type GenericComponent(Type parameter)
                => genericComponentType.MakeGenericType(parameter);

            componentTypes = componentParameterTypes
                .ToDictionary(t => t.Key, t => GenericComponent(t.Value));
        }

        protected override TComponentInterface ReadJson(JsonReader reader, JsonSerializer serializer)
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
            return (TComponentInterface) component;
        }
    }
}

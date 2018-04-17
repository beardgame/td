﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Game.Components;
using Bearded.TD.Mods.Serialization.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bearded.TD.Mods.Serialization.Converters
{
    static class ComponentConverterFactory
    {
        public static ComponentConverter<IBuildingComponent> ForBuildingComponents()
            => new ComponentConverter<IBuildingComponent>(typeof(BuildingComponent<>));

        public static ComponentConverter<IComponent> ForBaseComponent()
            => new ComponentConverter<IComponent>(typeof(Models.Component<>));
    }

    sealed class ComponentConverter<TComponentInterface> : JsonConverterBase<TComponentInterface>
    {
        private readonly Dictionary<string, Type> componentTypes;

        public ComponentConverter(Type genericComponentType)
        {
            componentTypes = ComponentFactories.ParameterTypesForComponentsById
                .ToDictionary(t => t.Key, t => genericComponentType.MakeGenericType(t.Value));
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

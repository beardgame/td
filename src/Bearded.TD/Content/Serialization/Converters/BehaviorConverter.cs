using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.Rules;

namespace Bearded.TD.Content.Serialization.Converters
{
    static class BehaviorConverterFactory
    {
        public static BehaviorConverter<IBuildingComponent> ForBuildingComponents()
            => new(ComponentFactories.ParameterTypesForComponentsById
                .ToDictionary(t => t.Key, t => typeof(BuildingComponent<>).MakeGenericType(t.Value)));

        public static BehaviorConverter<IComponent> ForBaseComponents()
            => new(ComponentFactories.ParameterTypesForComponentsById
                .ToDictionary(t => t.Key, t => typeof(Component<>).MakeGenericType(t.Value)));

        public static BehaviorConverter<IGameRule> ForGameRules()
            => new(GameRuleFactories.ParameterTypesForComponentsById
                .ToDictionary(t => t.Key, t => typeof(Models.GameRule<>).MakeGenericType(t.Value)));
    }

    sealed class BehaviorConverter<TComponentInterface> : JsonConverterBase<TComponentInterface>
    {
        private readonly Dictionary<string, Type> behaviorTypes;

        public override bool CanConvert(Type objectType)
        {
            // We don't want to use this converter if the specific type is known. Therefore IsAssignableFrom is too
            // open.
            return objectType == typeof(TComponentInterface);
        }

        public BehaviorConverter(Dictionary<string, Type> behaviorTypes)
        {
            this.behaviorTypes = behaviorTypes;
        }

        protected override TComponentInterface ReadJson(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var json = JsonDocument.ParseValue(ref reader);
            var root = json.RootElement;

            if (!root.TryGetProperty("id", out var id))
            {
                throw new JsonException("Behavior must have an id.");
            }

            if (!behaviorTypes.TryGetValue(id.GetString() ?? "", out var behaviorType))
            {
                throw new InvalidDataException($"Unknown Behavior id '{id}'.");
            }

            return (TComponentInterface) JsonSerializer.Deserialize(root.GetRawText(), behaviorType, options);
        }
    }
}

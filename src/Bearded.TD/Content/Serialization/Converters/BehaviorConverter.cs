using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Rules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using INodeBehavior = Bearded.TD.Content.Serialization.Models.INodeBehavior;

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

        public static BehaviorConverter<INodeBehavior> ForNodeBehaviors()
            => new(NodeBehaviorFactories.ParameterTypesForComponentsById
                .ToDictionary(t => t.Key, t => typeof(INodeBehavior).MakeGenericType(t.Value)));
    }

    sealed class BehaviorConverter<TComponentInterface> : JsonConverterBase<TComponentInterface>
    {
        private readonly Dictionary<string, Type> behaviorTypes;

        public BehaviorConverter(Dictionary<string, Type> behaviorTypes)
        {
            this.behaviorTypes = behaviorTypes;
        }

        protected override TComponentInterface ReadJson(JsonReader reader, JsonSerializer serializer)
        {
            var json = JObject.Load(reader);

            var id = json
                .GetValue("id", StringComparison.OrdinalIgnoreCase)
                ?.Value<string>();

            if (id == null)
                throw new InvalidDataException("Behavior must have an id.");

            if (!behaviorTypes.TryGetValue(id, out var behaviorType))
                throw new InvalidDataException($"Unknown Behavior id '{id}'.");

            var behavior = Activator.CreateInstance(behaviorType);
            serializer.Populate(json.CreateReader(), behavior);
            return (TComponentInterface) behavior;
        }
    }
}

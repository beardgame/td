using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Serialization.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IComponentModel = Bearded.TD.Content.Serialization.Models.IComponent;
using IFactionBehaviorModel = Bearded.TD.Content.Serialization.Models.IFactionBehavior;
using IGameRuleModel = Bearded.TD.Content.Serialization.Models.IGameRule;
using INodeBehaviorModel = Bearded.TD.Content.Serialization.Models.INodeBehavior;

namespace Bearded.TD.Content.Serialization.Converters;

static class BehaviorConverterFactory
{
    public static BehaviorConverter<IComponentModel> ForBaseComponents()
        => new(ComponentFactories.ParameterTypesForComponentsById
            .ToDictionary(t => t.Key, t => typeof(Component<>).MakeGenericType(t.Value)));

    public static BehaviorConverter<IFactionBehaviorModel> ForFactionBehaviors()
        => new(FactionBehaviorFactories.ParameterTypesForComponentsById
            .ToDictionary(t => t.Key, t => typeof(FactionBehavior<>).MakeGenericType(t.Value)));

    public static BehaviorConverter<IGameRuleModel> ForGameRules()
        => new(GameRuleFactories.ParameterTypesForComponentsById
            .ToDictionary(t => t.Key, t => typeof(GameRule<>).MakeGenericType(t.Value)));

    public static BehaviorConverter<INodeBehaviorModel> ForNodeBehaviors()
        => new(NodeBehaviorFactories.ParameterTypesForComponentsById
            .ToDictionary(t => t.Key, t => typeof(NodeBehavior<>).MakeGenericType(t.Value)));
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

        var behavior = Activator.CreateInstance(behaviorType)!;
        serializer.Populate(json.CreateReader(), behavior);
        return (TComponentInterface) behavior;
    }
}

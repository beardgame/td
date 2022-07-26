using System;
using System.Collections.Immutable;
using System.IO;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IComponent = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class UpgradeEffectConverter : JsonConverterBase<IUpgradeEffect>
{
    private enum UpgradeEffectType
    {
        Unknown = 0,
        Modification = 1,
        Component = 2,
        AddTags = 3,
    }

    protected override IUpgradeEffect ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        var json = JObject.Load(reader);

        var type = json
            .GetValue("type", StringComparison.OrdinalIgnoreCase)
            ?.ToObject<UpgradeEffectType>(serializer) ?? UpgradeEffectType.Unknown;
        var prerequisites = json
            .GetValue("prerequisites", StringComparison.OrdinalIgnoreCase)
            ?.ToObject<UpgradePrerequisites>(serializer) ?? UpgradePrerequisites.Empty;
        var isSideEffect = json
            .GetValue("isSideEffect", StringComparison.OrdinalIgnoreCase)
            ?.Value<bool>() ?? false;

        var def = json.GetValue("parameters");
        if (def == null)
        {
            throw new InvalidDataException("Missing upgrade effect definition.");
        }

        switch (type)
        {
            case UpgradeEffectType.Modification:
                var parameters = new ModificationParameters();
                serializer.Populate(def.CreateReader(), parameters);
                return new ParameterModifiable(
                    parameters.AttributeType, getModification(parameters), prerequisites, isSideEffect);
            case UpgradeEffectType.Component:
                var component = serializer.Deserialize<IComponent>(def.CreateReader());
                if (component == null)
                {
                    throw new InvalidDataException("Missing component definition");
                }
                return new ComponentModifiable(component, prerequisites, isSideEffect);
            case UpgradeEffectType.AddTags:
                var tags = serializer.Deserialize<ImmutableArray<string>>(def.CreateReader());
                if (tags == null || tags.IsEmpty)
                {
                    throw new InvalidDataException("Missing tags");
                }

                return new TagsModifiable(tags, prerequisites, isSideEffect);
            case UpgradeEffectType.Unknown:
            default:
                throw new InvalidDataException("Upgrade effect must have a valid type.");
        }
    }

    private static Modification getModification(ModificationParameters parameters)
    {
        return parameters.Mode switch
        {
            ModificationParameters.ModificationMode.Constant => Modification.AddConstant(parameters.Value),
            ModificationParameters.ModificationMode.FractionOfBase => Modification.AddFractionOfBase(parameters.Value),
            ModificationParameters.ModificationMode.Multiply => Modification.MultiplyWith(parameters.Value),
            _ => throw new InvalidDataException("Modification must have a valid type.")
        };
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    private sealed class ModificationParameters
    {
        public enum ModificationMode
        {
            Unknown = 0,
            Constant = 1,
            FractionOfBase = 2,
            Multiply = 3,
        }

        public AttributeType AttributeType;
        public ModificationMode Mode = ModificationMode.Unknown;
        public double Value;
    }
}

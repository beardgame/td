using System;
using System.IO;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class UpgradeEffectConverter : JsonConverterBase<IUpgradeEffect>
{
    private enum UpgradeEffectType
    {
        Unknown = 0,
        Modification = 1,
        Component = 2,
    }

    protected override IUpgradeEffect ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        var json = JObject.Load(reader);

        var type = json
            .GetValue("type", StringComparison.OrdinalIgnoreCase)
            ?.ToObject<UpgradeEffectType>(serializer) ?? UpgradeEffectType.Unknown;
        var prerequisites = json
            .GetValue("prerequisites", StringComparison.OrdinalIgnoreCase)
            ?.ToObject<UpgradePrerequisites>(serializer);

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
                return new ParameterModifiable(parameters.AttributeType, getModification(parameters), prerequisites);
            case UpgradeEffectType.Component:
                var component = serializer.Deserialize<IComponent>(def.CreateReader());
                if (component == null)
                {
                    throw new InvalidDataException("Missing component definition");
                }
                return new ComponentModifiable(component, prerequisites);
            case UpgradeEffectType.Unknown:
            default:
                throw new InvalidDataException($"Unsupported upgrade effect type: {type}");
        }
    }

    private static Modification getModification(ModificationParameters parameters)
    {
        switch (parameters.Mode)
        {
            case ModificationParameters.ModificationMode.Constant:
                return Modification.AddConstant(parameters.Value);
            case ModificationParameters.ModificationMode.FractionOfBase:
                return Modification.AddFractionOfBase(parameters.Value);
            case ModificationParameters.ModificationMode.Multiply:
                return Modification.MultiplyWith(parameters.Value);
            default:
                throw new InvalidDataException("Modification must have a valid type.");
        }
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

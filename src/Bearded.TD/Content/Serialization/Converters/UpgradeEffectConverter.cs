using System;
using System.IO;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
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

        switch (type)
        {
            case UpgradeEffectType.Modification:
                var parameters = new ModificationParameters();
                serializer.Populate(json.GetValue("parameters").CreateReader(), parameters);
                return new ParameterModifiable(parameters.AttributeType, getModification(parameters));
            case UpgradeEffectType.Component:
                var component = serializer.Deserialize<IComponent>(json.GetValue("parameters").CreateReader());
                return new ComponentModifiable(component);
            default:
                throw new InvalidDataException("Upgrade effect must have a valid type.");
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

    private class ModificationParameters
    {
        public enum ModificationMode
        {
            Unknown = 0,
            Constant = 1,
            FractionOfBase = 2,
            Multiply = 3,
        }

#pragma warning disable 649
        public AttributeType AttributeType;
        public ModificationMode Mode = ModificationMode.Unknown;
        public double Value;
#pragma warning restore 649
    }
}
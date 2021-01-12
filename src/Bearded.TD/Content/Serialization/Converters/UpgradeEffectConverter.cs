using System.IO;
using System.Text.Json;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class UpgradeEffectConverter : JsonConverterBase<IUpgradeEffect>
    {
        private enum UpgradeEffectType
        {
            Unknown = 0,
            Modification = 1,
            Component = 2,
        }

        protected override IUpgradeEffect ReadJson(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var json = JsonDocument.ParseValue(ref reader);

            var type = UpgradeEffectType.Unknown;
            if (json.RootElement.TryGetProperty("type", out var typeJson))
            {
                type = JsonSerializer.Deserialize<UpgradeEffectType>(typeJson.GetRawText(), options);
            }

            if (!json.RootElement.TryGetProperty("parameters", out var parametersJson))
            {
                parametersJson = new JsonElement();
            }

            switch (type)
            {
                case UpgradeEffectType.Modification:
                    var parameters =
                        JsonSerializer.Deserialize<ModificationParameters>(parametersJson.GetRawText(), options)
                        ?? throw new JsonException("Could not parse modification");
                    return new ParameterModifiable(parameters.AttributeType, getModification(parameters));
                case UpgradeEffectType.Component:
                    var component = JsonSerializer.Deserialize<IComponent>(parametersJson.GetRawText(), options)
                        ?? throw new JsonException("Could not parse component");
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

        [UsedImplicitly]
        private sealed class ModificationParameters
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
}

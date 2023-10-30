using Bearded.TD.Shared.TechEffects;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Converters;

sealed partial class UpgradeEffectConverter
{
    [UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    private sealed record ModificationParameters(
        AttributeType AttributeType, double Value, ModificationMode Mode = ModificationMode.Unknown);

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    private enum ModificationMode
    {
        Unknown = 0,
        Constant = 1,
        FractionOfBase = 2,
        Multiply = 3,
    }
}

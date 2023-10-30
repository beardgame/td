using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.Upgrades;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Converters;

sealed partial class UpgradeEffectConverter
{
    [UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    private sealed record TransactionParameters(
        IComponent ComponentToAdd, string KeyToRemove, TransactComponents.ReplaceMode ReplaceMode = TransactComponents.ReplaceMode.InsertOrReplace);
}

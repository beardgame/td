using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Upgrades;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class UpgradePrerequisitesConverter : JsonConverterBase<UpgradePrerequisites>
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    private sealed record NullablePrerequisites(
        ImmutableHashSet<string>? RequiredTags, ImmutableHashSet<string>? ForbiddenTags);

    protected override UpgradePrerequisites ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        var nullablePrerequisites = serializer.Deserialize<NullablePrerequisites>(reader);
        if (nullablePrerequisites == null)
        {
            return UpgradePrerequisites.Empty;
        }

        return new UpgradePrerequisites(
            nullablePrerequisites.RequiredTags ?? ImmutableHashSet<string>.Empty,
            nullablePrerequisites.ForbiddenTags ?? ImmutableHashSet<string>.Empty);
    }
}

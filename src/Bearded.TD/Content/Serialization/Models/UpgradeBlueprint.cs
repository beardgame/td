using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class UpgradeBlueprint
    : IConvertsTo<Content.Models.PermanentUpgrade, Void>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public Resource<Scrap> Cost { get; set; }
    public List<IUpgradeEffect>? Effects { get; set; }
    public Element Element { get; set; }

    public Content.Models.PermanentUpgrade ToGameModel(ModMetadata modMetadata, Void resolvers)
    {
        _ = Id ?? throw new InvalidDataException($"{nameof(Id)} must be non-null");
        _ = Name ?? throw new InvalidDataException($"{nameof(Name)} must be non-null");

        return new Content.Models.PermanentUpgrade(
            ModAwareId.FromNameInMod(Id, modMetadata),
            Name,
            Cost,
            Effects?.ToImmutableArray() ?? ImmutableArray<IUpgradeEffect>.Empty,
            Element);
    }
}

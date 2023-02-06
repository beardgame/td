using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class ModuleBlueprint
    : IConvertsTo<Content.Models.Module, Void>
{
    public string? Id { get; set; }
    public Element? AffinityElement { get; set; }
    public SocketShape? SocketShape { get; set; }
    public List<IUpgradeEffect>? Effects { get; set; }

    public Content.Models.Module ToGameModel(ModMetadata modMetadata, Void resolvers)
    {
        _ = Id ?? throw new InvalidDataException($"{nameof(Id)} must be non-null");
        _ = AffinityElement ?? throw new InvalidDataException($"{nameof(AffinityElement)} must be non-null");
        _ = SocketShape ?? throw new InvalidDataException($"{nameof(SocketShape)} must be non-null");

        return new Content.Models.Module(
            ModAwareId.FromNameInMod(Id, modMetadata),
            AffinityElement.Value,
            SocketShape.Value,
            Effects?.ToImmutableArray() ?? ImmutableArray<IUpgradeEffect>.Empty);
    }
}

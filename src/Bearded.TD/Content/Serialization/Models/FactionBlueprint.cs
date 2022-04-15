using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class FactionBlueprint : IConvertsTo<IFactionBlueprint, Void>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public Color? Color { get; set; }
    public List<IFactionBehavior>? Behaviors { get; set; }

    public IFactionBlueprint ToGameModel(ModMetadata modMetadata, Void resolvers)
    {
        return new Content.Models.FactionBlueprint(
            ExternalId<Faction>.FromLiteral(Id),
            Name,
            Color,
            Behaviors?.Select(FactionBehaviorFactories.CreateFactionBehaviorFactory)
            ?? Enumerable.Empty<IFactionBehaviorFactory>());
    }
}

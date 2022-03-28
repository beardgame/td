using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class GameModeBlueprint : IConvertsTo<IGameModeBlueprint, Void>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public List<IGameRule> Rules { get; set; } = new();

    public IGameModeBlueprint ToGameModel(ModMetadata modMetadata, Void _)
    {
        return new Content.Models.GameModeBlueprint(
            ModAwareId.FromNameInMod(Id ?? throw new InvalidDataException("Id must be non-null"), modMetadata),
            Name ?? throw new InvalidDataException("Name must be non-null"),
            Rules.Select(GameRuleFactories.CreateGameRuleFactory));
    }
}

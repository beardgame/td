using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Rules;

namespace Bearded.TD.Content.Models;

sealed class GameModeBlueprint : IGameModeBlueprint
{
    public ModAwareId Id { get; }
    public string Name { get; }
    public ImmutableArray<IGameRuleFactory> Rules { get; }

    public GameModeBlueprint(ModAwareId id, string name, IEnumerable<IGameRuleFactory> rules)
    {
        Id = id;
        Name = name;
        Rules = rules.ToImmutableArray();
    }
}

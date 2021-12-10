using System.Collections.Immutable;

namespace Bearded.TD.Game.Simulation.Rules;

interface IGameModeBlueprint : IBlueprint
{
    string Name { get; }
    ImmutableArray<IGameRuleFactory<GameState>> Rules { get; }
}
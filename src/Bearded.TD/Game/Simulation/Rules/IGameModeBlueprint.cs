using System.Collections.Immutable;
using Bearded.TD.Content.Models;

namespace Bearded.TD.Game.Simulation.Rules
{
    interface IGameModeBlueprint : IBlueprint
    {
        string Name { get; }
        ImmutableArray<IGameRuleFactory<GameState>> Rules { get; }
    }
}

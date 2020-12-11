using System.Collections.ObjectModel;

namespace Bearded.TD.Game.Simulation.Rules
{
    interface IGameModeBlueprint : IBlueprint
    {
        string Name { get; }
        ReadOnlyCollection<IGameRuleFactory<GameState>> Rules { get; }
    }
}

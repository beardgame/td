using System.Collections.ObjectModel;

namespace Bearded.TD.Game.Rules
{
    interface IGameModeBlueprint : IBlueprint
    {
        string Name { get; }
        ReadOnlyCollection<IGameRuleFactory<GameState>> Rules { get; }
    }
}

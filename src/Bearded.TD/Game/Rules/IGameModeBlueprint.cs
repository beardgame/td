using System.Collections.ObjectModel;

namespace Bearded.TD.Game.Rules
{
    interface IGameModeBlueprint : IBlueprint
    {
        string Id { get; }
        string Name { get; }
        ReadOnlyCollection<IGameRuleFactory<GameState>> Rules { get; }
    }
}

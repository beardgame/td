using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Rules;

namespace Bearded.TD.Content.Models
{
    sealed class GameModeBlueprint : IGameModeBlueprint
    {
        public string Id { get; }
        public string Name { get; }
        public ReadOnlyCollection<IGameRuleFactory<GameState>> Rules { get; }

        public GameModeBlueprint(string id, string name, IEnumerable<IGameRuleFactory<GameState>> rules)
        {
            Id = id;
            Name = name;
            Rules = (rules?.ToList() ?? new List<IGameRuleFactory<GameState>>()).AsReadOnly();
        }
    }
}

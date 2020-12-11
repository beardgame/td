using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Rules;

namespace Bearded.TD.Content.Models
{
    sealed class GameModeBlueprint : IGameModeBlueprint
    {
        public ModAwareId Id { get; }
        public string Name { get; }
        public ReadOnlyCollection<IGameRuleFactory<GameState>> Rules { get; }

        public GameModeBlueprint(ModAwareId id, string name, IEnumerable<IGameRuleFactory<GameState>> rules)
        {
            Id = id;
            Name = name;
            Rules = (rules?.ToList() ?? new List<IGameRuleFactory<GameState>>()).AsReadOnly();
        }
    }
}

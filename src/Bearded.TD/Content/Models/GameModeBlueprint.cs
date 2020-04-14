using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Components;

namespace Bearded.TD.Content.Models
{
    sealed class GameModeBlueprint : IBlueprint
    {
        public string Id { get; }
        public ReadOnlyCollection<IComponentFactory<GameState>> Rules { get; }

        public GameModeBlueprint(string id, IEnumerable<IComponentFactory<GameState>> rules)
        {
            Id = id;
            Rules = (rules?.ToList() ?? new List<IComponentFactory<GameState>>()).AsReadOnly();
        }
    }
}

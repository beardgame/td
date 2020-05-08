using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Rules;
using Bearded.Utilities;

namespace Bearded.TD.Content.Serialization.Models
{
    sealed class GameModeBlueprint : IConvertsTo<IGameModeBlueprint, Void>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<IGameRule> Rules { get; set; }

        public IGameModeBlueprint ToGameModel(Void _)
        {
            return new Content.Models.GameModeBlueprint(
                Id,
                Name,
                Rules?.Select(GameRuleFactories.CreateGameRuleFactory<GameState>));
        }
    }
}

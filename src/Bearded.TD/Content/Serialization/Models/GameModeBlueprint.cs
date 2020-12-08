using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.GameState;
using Bearded.TD.Game.GameState.Rules;
using Bearded.Utilities;

namespace Bearded.TD.Content.Serialization.Models
{
    sealed class GameModeBlueprint : IConvertsTo<IGameModeBlueprint, Void>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<IGameRule> Rules { get; set; }

        public IGameModeBlueprint ToGameModel(ModMetadata modMetadata, Void _)
        {
            return new Content.Models.GameModeBlueprint(
                ModAwareId.FromNameInMod(Id, modMetadata),
                Name,
                Rules?.Select(GameRuleFactories.CreateGameRuleFactory<GameState>));
        }
    }
}

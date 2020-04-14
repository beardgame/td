using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game;
using Bearded.Utilities;

namespace Bearded.TD.Content.Serialization.Models
{
    sealed class GameModeBlueprint : IConvertsTo<Content.Models.GameModeBlueprint, Void>
    {
        public string Id { get; set; }
        public List<IComponent> Rules { get; set; }

        public Content.Models.GameModeBlueprint ToGameModel(Void _)
        {
            return new Content.Models.GameModeBlueprint(
                Id,
                Rules?.Select(ComponentFactories.CreateComponentFactory<GameState>));
        }
    }
}

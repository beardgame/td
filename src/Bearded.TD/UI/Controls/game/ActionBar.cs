using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Mods.Models;
using Bearded.TD.UI.Input;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class ActionBar
    {
        public const int NumActions = 10;

        public event VoidEventHandler ActionsChanged;

        private readonly BuildingBlueprint[] blueprints = new BuildingBlueprint[NumActions];
        private GameInstance game;

        public void Initialize(GameInstance game)
        {
            this.game = game;

            foreach (var (blueprint, i) in game.State.Technology.UnlockedBuildings.Take(10).Indexed())
            {
                blueprints[i] = blueprint;
            }

            ActionsChanged?.Invoke();
        }

        public string ActionLabelForIndex(int i) => blueprints[i]?.Name;

        public void OnActionClicked(int actionIndex)
        {
            if (blueprints[actionIndex] == null) return;
            game.PlayerInput.SetInteractionHandler(new BuildingInteractionHandler(game, game.Me.Faction, blueprints[actionIndex]));
        }
    }
}

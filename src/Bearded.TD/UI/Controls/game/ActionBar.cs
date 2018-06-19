using Bearded.TD.Game;
using Bearded.TD.Mods.Models;
using Bearded.TD.UI.Input;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class ActionBar
    {
        private static readonly string[] buildings =
        {
            "wall",
            "triangle",
            "slowEmitter",
            "mine",
            "beamTurret"
        };

        public event VoidEventHandler ActionsChanged;

        private readonly BuildingBlueprint[] blueprints = new BuildingBlueprint[Constants.Game.UI.ActionBarSize];
        private GameInstance game;

        public void Initialize(GameInstance game)
        {
            this.game = game;

            for (var i = 0; i < buildings.Length; i++)
            {
                blueprints[i] = game.Blueprints.Buildings[buildings[i]];
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

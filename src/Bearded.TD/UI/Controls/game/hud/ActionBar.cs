using Bearded.TD.Game;
using Bearded.TD.Game.Input;
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
            "beamTurret",
            "sniperTurret"
        };

        public event VoidEventHandler ActionsChanged;

        private readonly InteractionHandler[] handlers = new InteractionHandler[Constants.Game.UI.ActionBarSize];
        private readonly string[] labels = new string[Constants.Game.UI.ActionBarSize];
        private GameInstance game;

        public void Initialize(GameInstance game)
        {
            this.game = game;

            for (var i = 0; i < buildings.Length; i++)
            {
                var blueprint = game.Blueprints.Buildings[buildings[i]];
                handlers[i] = new BuildingInteractionHandler(game, game.Me.Faction, blueprint);
                labels[i] = blueprint.Name;
            }

            handlers[Constants.Game.UI.ActionBarSize - 1] = new MiningInteractionHandler(game, game.Me.Faction);
            labels[Constants.Game.UI.ActionBarSize - 1] = "Mine tile";

            ActionsChanged?.Invoke();
        }

        public string ActionLabelForIndex(int i) => labels[i];

        public void OnActionClicked(int actionIndex)
        {
            if (handlers[actionIndex] == null) return;
            game.PlayerInput.SetInteractionHandler(handlers[actionIndex]);
        }
    }
}

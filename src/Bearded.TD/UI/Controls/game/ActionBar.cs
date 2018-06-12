using Bearded.TD.Game;
using Bearded.TD.UI.Input;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class ActionBar
    {
        public const int NumActions = 10;

        public event VoidEventHandler ActionsChanged;

        private readonly InteractionHandler[] actions;
        private GameInstance game;

        public void Initialize(GameInstance game)
        {
            this.game = game;
        }

        public string ActionLabelForIndex(int i) => "boo" + i;

        public void OnActionClicked(int actionIndex)
        {
            if (actions[actionIndex] == null) return;
            game.PlayerInput.SetInteractionHandler(actions[actionIndex]);
        }
    }
}

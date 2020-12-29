using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class GamePausedControl : CompositeControl
    {
        public event VoidEventHandler? ResumeGameButtonClicked;
        public event VoidEventHandler? ReturnToMainMenuButtonClicked;

        public GamePausedControl()
        {
            IsClickThrough = true;
            this.BuildLayout()
                .AddMenu(b => b
                    .WithCloseAction("To main menu", () => ReturnToMainMenuButtonClicked?.Invoke())
                    .AddMenuAction("Resume game", () => ResumeGameButtonClicked?.Invoke())
                    .AddMenuAction("Options", () => { }, new Binding<bool>(false)));
        }
    }
}

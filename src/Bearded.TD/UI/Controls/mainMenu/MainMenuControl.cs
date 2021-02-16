using Bearded.Graphics;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class MainMenuControl : CompositeControl
    {
        public MainMenuControl(MainMenu model)
        {
            Add(new BackgroundBox(Color.DarkSlateBlue));
            this.BuildLayout().AddMenu(b => b
                .AddMenuAction("Host game", model.OnHostGameButtonClicked)
                .AddMenuAction("Join game", model.OnJoinGameButtonClicked)
                .AddMenuAction("Options", () => { }, new Binding<bool>(false))
                .WithCloseAction("Exit", model.OnQuitGameButtonClicked));
        }
    }
}

using Bearded.TD.UI.Factories;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed class MainMenuControl : CompositeControl
{
    public MainMenuControl(MainMenu model)
    {
        Add(new MainMenuBackground());

        this.BuildLayout().AddMenu(b => b
            .AddMenuAction("Quick game", model.OnQuickGameButtonClicked)
            .AddMenuAction("Host game", model.OnHostGameButtonClicked)
            .AddMenuAction("Join game", model.OnJoinGameButtonClicked)
            .AddMenuAction("Options", model.OnOptionsButtonClicked)
            .WithCloseAction("Exit", model.OnQuitGameButtonClicked));
    }
}

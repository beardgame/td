using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Factories;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed class MainMenuControl : CompositeControl
{
    public MainMenuControl(MainMenu model, Animations animations)
    {
        Add(new MainMenuBackground());

        var menuFrame = new CompositeControl();
        Add(menuFrame);

        menuFrame.BuildLayout().AddMenu(b => b
            .AddMenuAction("Quick game", model.OnQuickGameButtonClicked)
            .AddMenuAction("Host game", model.OnHostGameButtonClicked)
            .AddMenuAction("Join game", model.OnJoinGameButtonClicked)
            .AddMenuAction("Options", model.OnOptionsButtonClicked)
            .WithCloseAction("Exit", model.OnQuitGameButtonClicked)
            .WithAnimations(animations)
        );

        animations.Start(Constants.UI.Menu.SlideInAnimation, menuFrame);
    }
}

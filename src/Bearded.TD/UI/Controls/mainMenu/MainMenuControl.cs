using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Layers;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed class MainMenuControl : CompositeControl
{
    public MainMenuControl(MainMenu model, UIContext context)
    {
        Add(new MainMenuBackground());

        var menuFrame = OnTopCompositeControl.CreateClickThrough("Main Menu Frame");
        Add(menuFrame);

        menuFrame.BuildLayout().AddMenu(context.Factories, b => b
            .AddMenuAction("Quick game", model.OnQuickGameButtonClicked)
            .AddMenuAction("Host game", model.OnHostGameButtonClicked)
            .AddMenuAction("Join game", model.OnJoinGameButtonClicked)
            .AddMenuAction("Options", model.OnOptionsButtonClicked)
            .WithCloseAction("Exit", model.OnQuitGameButtonClicked)
            .WithBlurredBackground()
        );

        context.Animations.Start(Constants.UI.Menu.SlideInAnimation, menuFrame);
    }
}

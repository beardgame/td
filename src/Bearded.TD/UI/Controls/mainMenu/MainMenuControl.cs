using Bearded.TD.Rendering.UI.Gradients;
using Bearded.TD.UI.Factories;
using Bearded.UI.Controls;
using static Bearded.TD.Constants.UI;

namespace Bearded.TD.UI.Controls;

sealed class MainMenuControl : CompositeControl
{
    public MainMenuControl(MainMenu model)
    {
        Add(new BackgroundBox(Colors.Get(BackgroundColor.MainMenuBackground)));
        this.BuildLayout().AddMenu(b => b
            .WithBackground(new ComplexBox
            {
                FillColor = Colors.Get(BackgroundColor.Default),
                GlowOuterWidth = 15,
                GlowOuterColor = GradientParameters.SimpleGlow(Shadows.Default.Color * 0.5f),
            })
            .AddMenuAction("Quick game", model.OnQuickGameButtonClicked)
            .AddMenuAction("Host game", model.OnHostGameButtonClicked)
            .AddMenuAction("Join game", model.OnJoinGameButtonClicked)
            .AddMenuAction("Options", model.OnOptionsButtonClicked)
            .WithCloseAction("Exit", model.OnQuitGameButtonClicked));
    }
}

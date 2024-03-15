using Bearded.TD.UI.Factories;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

sealed class MainMenuControl : CompositeControl
{
    public MainMenuControl(MainMenu model)
    {
        Add(new MainMenuBackground().Anchor(
            a => a.Right(50)));

        //Add(new Sprite { SpriteId = Constants.Content.CoreUI.MainMenu.Turret, Size = 500 }.Anchor(
        //    a => a.Bottom(-20, 300).Left(-20, 500)));

        this.BuildLayout().AddMenu(b => b
            .AddMenuAction("Quick game", model.OnQuickGameButtonClicked)
            .AddMenuAction("Host game", model.OnHostGameButtonClicked)
            .AddMenuAction("Join game", model.OnJoinGameButtonClicked)
            .AddMenuAction("Options", model.OnOptionsButtonClicked)
            .WithCloseAction("Exit", model.OnQuitGameButtonClicked));
    }
}

sealed class MainMenuBackground : Control
{
    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}

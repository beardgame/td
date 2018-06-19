using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class MainMenuView : CompositeControl
    {
        public MainMenuView(MainMenu model)
        {
            Add(
                new CompositeControl() // ButtonGroup
                {
                    new Button {new Label("Host game")}
                        .Anchor(a => a.Top(margin: 0, height: 50))
                        .Subscribe(b => b.Clicked += model.OnHostGameButtonClicked),
                    new Button {new Label("Join game")}
                        .Anchor(a => a.Top(margin: 50, height: 50))
                        .Subscribe(b => b.Clicked += model.OnJoinGameButtonClicked),
                    new Button {new Label("Exit")}
                        .Anchor(a => a.Top(margin: 100, height: 50))
                        .Subscribe(b => b.Clicked += model.OnQuitGameButtonClicked)
                }.Anchor(a => a.Right(margin: 20, width: 250).Bottom(margin: 20, height: 150))
            );
        }
    }
}

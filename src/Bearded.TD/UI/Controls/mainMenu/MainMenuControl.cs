using Bearded.UI.Controls;
using static Bearded.TD.UI.Factories.LegacyDefault;

namespace Bearded.TD.UI.Controls
{
    sealed class MainMenuControl : CompositeControl
    {
        public MainMenuControl(MainMenu model)
        {
            Add(
                new CompositeControl() // ButtonGroup
                {
                    Button("Host game")
                        .Anchor(a => a.Top(margin: 0, height: 50))
                        .Subscribe(b => b.Clicked += _ => model.OnHostGameButtonClicked()),
                    Button("Join game")
                        .Anchor(a => a.Top(margin: 50, height: 50))
                        .Subscribe(b => b.Clicked += _ => model.OnJoinGameButtonClicked()),
                    Button("Exit")
                        .Anchor(a => a.Top(margin: 100, height: 50))
                        .Subscribe(b => b.Clicked += _ => model.OnQuitGameButtonClicked())
                }.Anchor(a => a.Right(margin: 20, width: 250).Bottom(margin: 20, height: 150))
            );
        }
    }
}

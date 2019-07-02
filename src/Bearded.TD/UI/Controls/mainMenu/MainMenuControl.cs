using Bearded.UI.Controls;
using OpenTK.Input;
using static Bearded.TD.UI.Controls.Default;

namespace Bearded.TD.UI.Controls
{
    sealed class MainMenuControl : CompositeControl
    {
        public MainMenuControl(MainMenu model)
        {
            var menuList = new FocusList.Builder()
                .AddBackwardKey(Key.Up)
                .AddForwardKey(Key.Down)
                .MakeCyclic()
                .Build();

            menuList.Add(Button("Host game")
                .Anchor(a => a.Top(margin: 0, height: 50))
                .Subscribe(b => b.Clicked += model.OnHostGameButtonClicked));
            menuList.Add(Button("Join game")
                .Anchor(a => a.Top(margin: 50, height: 50))
                .Subscribe(b => b.Clicked += model.OnJoinGameButtonClicked));
            menuList.Add(Button("Exit")
                .Anchor(a => a.Top(margin: 100, height: 50))
                .Subscribe(b => b.Clicked += model.OnQuitGameButtonClicked));

            Add(menuList.Anchor(a => a.Right(margin: 20, width: 250).Bottom(margin: 20, height: 150)));

            model.Activated += menuList.Focus;
        }
    }
}

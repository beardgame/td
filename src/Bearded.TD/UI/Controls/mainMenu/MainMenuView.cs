using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class MainMenuView : CompositeControl
    {
        public event VoidEventHandler HostGameClicked;
        public event VoidEventHandler JoinGameClicked;
        public event VoidEventHandler QuitGameClicked;

        public MainMenuView()
        {
            Add(
                new CompositeControl() // ButtonGroup
                {
                    new LabeledButton<string>("Host game")
                        .Anchor(a => a.Top(margin: 0, height: 50))
                        .Subscribe(b => b.Clicked += onHostGameButtonClicked),
                    new LabeledButton<string>("Join game")
                        .Anchor(a => a.Top(margin: 50, height: 50))
                        .Subscribe(b => b.Clicked += onJoinGameButtonClicked),
                    new LabeledButton<string>("Exit")
                        .Anchor(a => a.Top(margin: 100, height: 50))
                        .Subscribe(b => b.Clicked += onQuitGameButtonClicked)
                }.Anchor(a => a.Right(margin: 20, width: 250).Bottom(margin: 20, height: 200))
            );
        }

        private void onHostGameButtonClicked() => HostGameClicked?.Invoke();
        private void onJoinGameButtonClicked() => JoinGameClicked?.Invoke();
        private void onQuitGameButtonClicked() => QuitGameClicked?.Invoke();
    }
}

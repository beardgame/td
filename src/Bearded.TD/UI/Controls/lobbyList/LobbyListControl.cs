using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class LobbyListControl : CompositeControl
    {
        public LobbyListControl(LobbyList model)
        {
            Add(new Button {new Label("Back to menu")}
                .Anchor(a => a
                    .Bottom(margin: 20, height: 50)
                    .Left(margin: 20, width: 250))
                .Subscribe(b => b.Clicked += model.OnBackToMenuButtonClicked));

            var manualTextInput = new TextInput();

            Add(new CompositeControl
            {
                manualTextInput.Anchor(a => a.Right(margin: 120)),
                new Button {new Label("Connect")}
                    .Anchor(a => a.Right(width: 120))
                    .Subscribe(b => b.Clicked += () => model.OnConnectManualButtonClicked(manualTextInput.Text))
            }.Anchor(a => a
                .Bottom(margin: 46, height: 24)
                .Right(margin: 20)
                .Left(relativePercentage: .5, margin: 20)));
        }
    }
}

using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class LobbyView : CompositeControl
    {
        public LobbyView(Lobby model)
        {
            Add(
                new CompositeControl() // ButtonGroup
                {
                    new LabeledButton<string>("Toggle ready")
                        .Anchor(a => a.Top(margin: 0, height: 50))
                        .Subscribe(b => b.Clicked += model.OnToggleReadyButtonClicked),
                    new LabeledButton<string>("Back to menu")
                        .Anchor(a => a.Top(margin: 50, height: 50))
                        .Subscribe(b => b.Clicked += model.OnBackToMenuButtonClicked),
                }.Anchor(a => a.Left(margin: 20, width: 250).Bottom(margin: 20, height: 100))
            );
        }
    }
}

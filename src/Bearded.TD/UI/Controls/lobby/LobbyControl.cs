using Bearded.UI.Controls;
using static Bearded.TD.UI.Controls.Default;

namespace Bearded.TD.UI.Controls
{
    sealed class LobbyControl : CompositeControl
    {
        public LobbyControl(Lobby model)
        {
            // main buttons
            Add(
                new CompositeControl // ButtonGroup
                {
                    Button("Toggle ready")
                        .Anchor(a => a.Top(margin: 0, height: 50))
                        .Subscribe(b => b.Clicked += model.OnToggleReadyButtonClicked),
                    Button("Back to menu")
                        .Anchor(a => a.Top(margin: 50, height: 50))
                        .Subscribe(b => b.Clicked += model.OnBackToMenuButtonClicked),
                }.Anchor(a => a.Left(margin: 20, width: 250).Bottom(margin: 20, height: 100))
            );

            // game settings
            Add(
                new CompositeControl // ButtonGroup
                {
                    new NumericInput(model.LevelSize)
                    {
                        MinValue = 10,
                        MaxValue = 100,
                        IsEnabled = !model.CanChangeGameSettings
                    }
                    .Anchor(a => a.Top(margin: 0, height: 50))
                        .Subscribe(b => b.ValueChanged += model.OnSetLevelSize),
                }.Anchor(a => a.Left(margin: 20, width: 250).Top(margin: 20, height: 100))
            );

            var list = new ListControl {ItemSource = new PlayerListItemSource(model)}
                .Anchor(a => a
                    .Left(relativePercentage: .5)
                    .Right(margin: 20)
                    .Top(margin: 20)
                    .Bottom(margin: 20));
            Add(list);
            model.PlayersChanged += list.Reload;
        }

        private class PlayerListItemSource : IListItemSource
        {
            private readonly Lobby lobby;

            public int ItemCount => lobby.Players.Count;

            public PlayerListItemSource(Lobby lobby)
            {
                this.lobby = lobby;
            }

            public double HeightOfItemAt(int index) => LobbyPlayerRowControl.Height;

            public Control CreateItemControlFor(int index) => new LobbyPlayerRowControl(lobby.Players[index]);

            public void DestroyItemControlAt(int index, Control control) { }
        }
    }
}

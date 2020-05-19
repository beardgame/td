using Bearded.TD.Meta;
using Bearded.UI.Controls;
using static Bearded.TD.UI.Factories.LegacyDefault;

namespace Bearded.TD.UI.Controls
{
    sealed class LobbyListControl : CompositeControl
    {
        public LobbyListControl(LobbyList model)
        {
            Add(Button("Refresh lobbies")
                .Anchor(a => a
                    .Bottom(margin: 70, height: 50)
                    .Left(margin: 20, width: 250))
                .Subscribe(b => b.Clicked += model.OnRefreshLobbiesButtonClicked));
            Add(Button("Back to menu")
                .Anchor(a => a
                    .Bottom(margin: 20, height: 50)
                    .Left(margin: 20, width: 250))
                .Subscribe(b => b.Clicked += model.OnBackToMenuButtonClicked));

            var manualTextInput = new TextInput { Text = UserSettings.Instance.Misc.SavedNetworkAddress };
            manualTextInput.MoveCursorToEnd();

            Add(new CompositeControl
            {
                manualTextInput.Anchor(a => a.Right(margin: 120)),
                Button("Connect")
                    .Anchor(a => a.Right(width: 120))
                    .Subscribe(b => b.Clicked += () => model.OnConnectManualButtonClicked(manualTextInput.Text))
            }.Anchor(a => a
                .Bottom(margin: 46, height: 24)
                .Right(margin: 20)
                .Left(relativePercentage: .5, margin: 20)));

            var list = new ListControl {ItemSource = new LobbyListItemSource(model)}
                .Anchor(a => a
                    .Left(relativePercentage: .5)
                    .Right(margin: 20)
                    .Top(margin: 20)
                    .Bottom(margin: 100));

            Add(list);
            model.LobbyReceived += lobby => list.OnAppendItems(1);
        }

        private class LobbyListItemSource : IListItemSource
        {
            private readonly LobbyList lobbyList;

            public int ItemCount => lobbyList.Lobbies.Count;

            public LobbyListItemSource(LobbyList lobbyList)
            {
                this.lobbyList = lobbyList;
            }

            public double HeightOfItemAt(int index) => LobbyListRowControl.Height;

            public Control CreateItemControlFor(int index)
            {
                var ctrl = new LobbyListRowControl(lobbyList.Lobbies[index]);
                ctrl.Clicked += lobby => lobbyList.OnLobbyClicked(lobby.Id);
                return ctrl;
            }

            public void DestroyItemControlAt(int index, Control control) { }
        }
    }
}

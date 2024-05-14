using Bearded.TD.Meta;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed class LobbyListControl : CompositeControl
{
    public LobbyListControl(LobbyList model, UIFactories factories)
    {
        var ipBinding = Binding.Create(UserSettings.Instance.Misc.SavedNetworkAddress);

        var list = new ListControl {ItemSource = new LobbyListItemSource(model)};
        model.LobbyReceived += _ => list.OnAppendItems(1);

        this.BuildLayout()
            .ForFullScreen()
            .AddNavBar(factories, b => b
                .WithBackButton("Back to menu", model.OnBackToMenuButtonClicked))
            .AddMainSidebar(c => c.BuildFixedColumn()
                .AddForm(factories, f => f
                    .AddTextInputRow("Custom IP", ipBinding)
                    .AddButtonRow("Connect", () => model.OnConnectManualButtonClicked(ipBinding.Value))
                    .AddButtonRow("Refresh lobbies", model.OnRefreshLobbiesButtonClicked)))
            .FillContent(list);
    }

    private sealed class LobbyListItemSource : IListItemSource
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

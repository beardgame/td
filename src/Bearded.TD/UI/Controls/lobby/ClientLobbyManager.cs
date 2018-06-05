using Bearded.TD.Game;
using Bearded.TD.Networking;
using Bearded.TD.UI.Model.Loading;

namespace Bearded.TD.UI.Controls
{
    class ClientLobbyManager : LobbyManager
    {
        public ClientLobbyManager(GameInstance game, NetworkInterface networkInterface)
            : base(game, networkInterface) {}

        public override LoadingManager GetLoadingManager() => new ClientLoadingManager(Game, Network);
    }
}

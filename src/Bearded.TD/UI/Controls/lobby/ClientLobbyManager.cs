using Bearded.TD.Game;
using Bearded.TD.Networking;

namespace Bearded.TD.UI.Controls
{
    class ClientLobbyManager : LobbyManager
    {
        public ClientLobbyManager(GameInstance game, NetworkInterface networkInterface)
            : base(game, networkInterface) {}

        public override LoadingManager GetLoadingManager(GameSettings gameSettings)
            => new ClientLoadingManager(Game, Network);
    }
}

﻿using Bearded.TD.Game;
using Bearded.TD.Networking;

namespace Bearded.TD.UI.Controls;

sealed class ClientLobbyManager : LobbyManager
{
    public override bool CanChangeGameSettings => false;
        
    public ClientLobbyManager(GameInstance game, NetworkInterface networkInterface)
        : base(game, networkInterface) {}

    public override LoadingManager GetLoadingManager() => new ClientLoadingManager(Game, Network);
}
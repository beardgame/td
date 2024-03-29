﻿using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content;
using Bearded.TD.Game;
using Bearded.TD.Game.Loading;
using Bearded.TD.Game.Players;
using Bearded.TD.Meta;
using Bearded.TD.Networking;
using Bearded.TD.Networking.MasterServer;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.IO;

namespace Bearded.TD.UI.Controls;

sealed class ServerLobbyManager : LobbyManager
{
    private const float heartbeatTimeSeconds = 10;

    private readonly ServerMasterServer masterServer;
    private float secondsUntilNextHeartbeat;
    private bool forceReady;

    public override bool CanChangeGameSettings => true;

    private ServerLobbyManager(GameInstance game, ServerNetworkInterface networkInterface)
        : base(game, networkInterface)
    {
        masterServer = networkInterface.Master;

        // First heartbeat automatically registers lobby.
        doHeartbeat();
    }

    public override void Update(UpdateEventArgs args)
    {
        base.Update(args);

        if (forceReady && Game.Content.IsFinishedLoading)
        {
            Game.Me.ConnectionState = PlayerConnectionState.Ready;
        }

        if (Game.Players.All(p => p.ConnectionState == PlayerConnectionState.Ready))
        {
            Dispatcher.RunOnlyOnServer(
                commandDispatcher => commandDispatcher.Dispatch(BeginLoadingGame.Command(Game)));
        }

        secondsUntilNextHeartbeat -= args.ElapsedTimeInSf;
        if (secondsUntilNextHeartbeat <= 0)
        {
            doHeartbeat();
        }
    }

    private void doHeartbeat()
    {
        masterServer.RegisterLobby(
            new Proto.Lobby
            {
                Id = Network.UniqueIdentifier,
                Name = $"{Game.Me.Name}'s game",
                MaxNumPlayers = 4,
                CurrentNumPlayers = 1,
            }
        );
        secondsUntilNextHeartbeat = heartbeatTimeSeconds;
    }

    public override LoadingManager GetLoadingManager() => new ServerLoadingManager(Game, Network);

    public static ServerLobbyManager CreateWithReadyPlayer(
        ServerNetworkInterface networkInterface,
        Logger logger,
        ContentManager contentManager,
        RenderContext renderContext)
    {
        var m = Create(networkInterface, logger, contentManager, renderContext);

        if (!m.Game.Content.EnabledMods.IsEmpty)
        {
            m.forceReady = true;
        }

        return m;
    }

    public static ServerLobbyManager Create(
        ServerNetworkInterface networkInterface,
        Logger logger,
        ContentManager contentManager,
        RenderContext renderContext)
    {
        var gameContent = new GameContent(contentManager);
        gameContent.SetEnabledModsById(UserSettings.Instance.LastGameSettings.ActiveModIds);

        var ids = new IdManager();
        var p = new Player(ids.GetNext<Player>(), UserSettings.Instance.Misc.Username)
        {
            ConnectionState = PlayerConnectionState.Waiting
        };

        return new ServerLobbyManager(
            new GameInstance(new ServerGameContext(networkInterface, logger), gameContent, p, ids, renderContext),
            networkInterface);
    }
}

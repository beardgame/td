﻿using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Players;
using Bearded.TD.Meta;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.UI.Navigation;
using Bearded.Utilities.IO;
using Lidgren.Network;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.UI.Controls
{
    sealed class LobbyList : UpdateableNavigationNode<Void>, INetworkMessageHandler
    {
        private Logger logger;
        private ContentManager contentManager;
        private ClientNetworkInterface networkInterface;

        protected override void Initialize(DependencyResolver dependencies, Void parameters)
        {
            base.Initialize(dependencies, parameters);

            logger = dependencies.Resolve<Logger>();
            contentManager = dependencies.Resolve<ContentManager>();

            networkInterface = new ClientNetworkInterface();
            networkInterface.RegisterMessageHandler(new NetworkDebugMessageHandler(logger));
            networkInterface.RegisterMessageHandler(this);
        }

        public override void Terminate()
        {
            base.Terminate();

            networkInterface.UnregisterMessageHandler(this);
        }

        public override void Update(UpdateEventArgs args)
        {
            networkInterface.ConsumeMessages();
        }

        public bool Accepts(NetIncomingMessage message)
            => message.MessageType == NetIncomingMessageType.StatusChanged
                || message.MessageType == NetIncomingMessageType.UnconnectedData
                || message.MessageType == NetIncomingMessageType.NatIntroductionSuccess;

        public void Handle(NetIncomingMessage message)
        {
            switch (message.MessageType)
            {
                case NetIncomingMessageType.StatusChanged:
                    handleStatusChange(message);
                    break;
                case NetIncomingMessageType.UnconnectedData:
                    // Data coming from an unconnected source. Must be master server.	
                    handleIncomingLobby(Proto.Lobby.Parser.ParseFrom(message.ReadBytes(message.LengthBytes)));
                    break;
                case NetIncomingMessageType.NatIntroductionSuccess:
                    // NAT introduction success. Means we successfully connected with lobby.	
                    networkInterface.Connect(message.SenderEndPoint, new ClientInfo(playerName));
                    break;
            }
        }

        private void handleIncomingLobby(Proto.Lobby lobby)
        {

        }

        private void handleStatusChange(NetIncomingMessage msg)
        {
            var status = (NetConnectionStatus) msg.ReadByte(); // Read status byte.
            switch (status)
            {
                case NetConnectionStatus.Connected:
                    goToLobby(msg);
                    return;
                case NetConnectionStatus.Disconnected:
                    var rejectionReason = msg.ReadString();
                    logger.Info.Log(string.IsNullOrEmpty(rejectionReason)
                        ? "Disconnected"
                        : $"Disconnected with reason: {rejectionReason}");
                    networkInterface.Shutdown();
                    break;
            }
        }

        private void goToLobby(NetIncomingMessage msg)
        {
            var info = LobbyPlayerInfo.FromBuffer(msg.SenderConnection.RemoteHailMessage);
            var game = new GameInstance(
                new ClientGameContext(networkInterface, logger),
                contentManager,
                new Player(info.Id, playerName),
                null);

            Navigation.Replace<Lobby, LobbyManager>(
                new ClientLobbyManager(game, networkInterface), this);
        }

        public void OnConnectManualButtonClicked(string host)
        {
            networkInterface.Connect(host, new ClientInfo(playerName));

            UserSettings.Instance.Misc.SavedNetworkAddress = host;
            UserSettings.Save(logger);
        }

        public void OnBackToMenuButtonClicked()
        {
            networkInterface.Shutdown();
            Navigation.Replace<MainMenu>(this);
        }

        private static string playerName
            => UserSettings.Instance.Misc.Username?.Length > 0
                ? UserSettings.Instance.Misc.Username
                : "player";
    }
}

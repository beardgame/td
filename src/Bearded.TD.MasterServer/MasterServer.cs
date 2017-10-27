using System;
using System.Collections.Generic;
using Lidgren.Network;

namespace Bearded.TD.MasterServer
{
    class MasterServer
    {
        private readonly object lobbyIdLock = new object();

        // TODO: use IdManager once I have internet. Also add a logger maybe.
        private int nextLobbyId = 1;

		private const string applicationName = "Bearded.TD";
		private const int defaultPort = 24292;

        private readonly NetServer server;

        private readonly ISet<Proto.Lobby> pendingLobbies = new HashSet<Proto.Lobby>();
        private readonly IList<Proto.Lobby> activeLobbies = new List<Proto.Lobby>();
        private readonly IDictionary<int, Proto.Lobby> lobbiesById = new Dictionary<int, Proto.Lobby>();
        private readonly IDictionary<NetConnection, int> idByConnection = new Dictionary<NetConnection, int>();

        public MasterServer()
        {
			var config = new NetPeerConfiguration(applicationName)
			{
				Port = defaultPort
			};
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
			server = new NetServer(config);
			server.Start();

            server.RegisterReceivedCallback(handleIncomingMessages);
        }

        private void handleIncomingMessages(object state)
        {
            var msg = server.ReadMessage();
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.ConnectionApproval:
                    handleConnectionApproval(msg);
                    break;
                case NetIncomingMessageType.StatusChanged:
                    handleStatusChange(msg);
                    break;
                case NetIncomingMessageType.Data:
                    handleData(msg);
                    break;
                case NetIncomingMessageType.UnconnectedData:
                    handleUnconnectedData(msg);
                    break;
            }
        }

        private void handleConnectionApproval(NetIncomingMessage msg)
        {
            var request = Proto.RegisterLobbyRequest.Parser.ParseFrom(
                msg.SenderConnection.RemoteHailMessage.ReadBytes(msg.LengthBytes));
            lock (lobbyIdLock)
            {
                var pendingLobby = new Proto.Lobby(request.Lobby)
                {
                    Id = nextLobbyId++
                };
                pendingLobbies.Add(pendingLobby);
                lobbiesById.Add(pendingLobby.Id, pendingLobby);
                idByConnection.Add(msg.SenderConnection, pendingLobby.Id);
            }
        }

        private void handleStatusChange(NetIncomingMessage msg)
        {
            // TODO: Is this right? Look up when I have internet.
            var status = (NetConnectionStatus)msg.ReadByte();
            var lobby = lobbiesById[idByConnection[msg.SenderConnection]];
            switch (status)
            {
                case NetConnectionStatus.Connected:
                    if (!pendingLobbies.Contains(lobby))
                    {
                        throw new Exception("Can only connect after approval.");
                    }
                    pendingLobbies.Remove(lobby);
                    activeLobbies.Add(lobby);
                    break;
                case NetConnectionStatus.Disconnected:
                    pendingLobbies.Remove(lobby);
                    activeLobbies.Remove(lobby);
                    lobbiesById.Remove(lobby.Id);
                    idByConnection.Remove(msg.SenderConnection);
                    break;
            }
        }

        private void handleData(NetIncomingMessage msg)
        {
            
        }

        private void handleUnconnectedData(NetIncomingMessage msg)
		{
            
		}
    }
}

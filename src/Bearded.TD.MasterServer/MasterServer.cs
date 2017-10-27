using System;
using System.Collections.Generic;
using Google.Protobuf;
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
        private readonly IDictionary<NetConnection, int> idsByConnection = new Dictionary<NetConnection, int>();
        private readonly IDictionary<int, NetConnection> connectionsById = new Dictionary<int, NetConnection>();

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
            var request = Proto.HailRequest.Parser.ParseFrom(
                msg.SenderConnection.RemoteHailMessage.ReadBytes(msg.LengthBytes));
            lock (lobbyIdLock)
            {
                var pendingLobby = new Proto.Lobby(request.Lobby)
                {
                    Id = nextLobbyId++
                };
                pendingLobbies.Add(pendingLobby);
                lobbiesById.Add(pendingLobby.Id, pendingLobby);
                idsByConnection.Add(msg.SenderConnection, pendingLobby.Id);
                connectionsById.Add(pendingLobby.Id, msg.SenderConnection);

                var response = new Proto.HailResponse
                {
                    AssignedId = pendingLobby.Id
                };
                var responseMsg = server.CreateMessage();
                responseMsg.Write(response.ToByteArray());
                msg.SenderConnection.Approve(responseMsg);
            }
        }

        private void handleStatusChange(NetIncomingMessage msg)
        {
            // TODO: Is this right? Look up when I have internet.
            var status = (NetConnectionStatus)msg.ReadByte();
            var lobby = lobbiesById[idsByConnection[msg.SenderConnection]];
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
                    dropLobby(lobby, msg.SenderConnection);
                    break;
            }
        }

        private void handleData(NetIncomingMessage msg)
        {
            // This is from a connection, so that means a lobby.
            // 2 possibilities: lobby is being updated, or lobby is being closed.
            var request = Proto.LobbyRequest.Parser.ParseFrom(msg.ReadBytes(msg.LengthBytes));
            var expectedConnection = connectionsById[request.LobbyId];
            if (expectedConnection != msg.SenderConnection)
            {
                throw new Exception("Unexpected connection for lobby id.");
            }

            if (request.Lobby == null)
            {
                // Drop lobby and connection.
                dropLobby(lobbiesById[request.LobbyId], msg.SenderConnection);
                msg.SenderConnection.Disconnect("goodbye");
            }
            else
            {
                // Update lobby info.
                lobbiesById[request.LobbyId].EnabledMod.Clear();
                lobbiesById[request.LobbyId].MergeFrom(request.Lobby);
            }

            // TODO: respond?
        }

        private void dropLobby(Proto.Lobby lobby, NetConnection connection)
        {
			pendingLobbies.Remove(lobby);
			activeLobbies.Remove(lobby);
			lobbiesById.Remove(lobby.Id);
			idsByConnection.Remove(connection);
            connectionsById.Remove(lobby.Id);
        }

        private void handleUnconnectedData(NetIncomingMessage msg)
		{
			// This is not from a connection, so that means a peer.
			// 2 possibilities: peer wants a list of lobbies, or peer wants an introduction.
			// NOTE: in the future, we may want to show a list of peers in the lobby, which means
			//       we need to handle peers as connections as well.
            var request = Proto.PeerRequest.Parser.ParseFrom(msg.ReadBytes(msg.LengthBytes));

            if (request.LobbyId > 0)
            {
                // Connect to lobby.
                var hostConnection = connectionsById[request.LobbyId];
                server.Introduce(null, hostConnection.RemoteEndPoint, null, msg.SenderEndPoint, "hi");
            }
            else
            {
                // Send list of lobbies.
                var response = new Proto.PeerResponse();
                response.Lobby.Add(activeLobbies);
                var responseMsg = server.CreateMessage();
                responseMsg.Write(response.ToByteArray());
                server.SendUnconnectedMessage(responseMsg, msg.SenderEndPoint);
            }
		}
    }
}

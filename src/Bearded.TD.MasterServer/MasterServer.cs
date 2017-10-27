using System.Collections.Generic;
using System.Net;
using Bearded.Utilities;
using Google.Protobuf;
using Lidgren.Network;

namespace Bearded.TD.MasterServer
{
    class MasterServer
    {
        private readonly object lobbyIdLock = new object();

		private const string applicationName = "Bearded.TD.Master";
		private const int masterServerPort = 24293;

        private readonly Logger logger;
        private readonly NetPeer peer;

        private readonly Dictionary<long, Lobby> lobbiesById = new Dictionary<long, Lobby>();

        public MasterServer(Logger logger)
        {
            this.logger = logger;
            var config = new NetPeerConfiguration(applicationName)
			{
				Port = masterServerPort
			};
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            peer = new NetPeer(config);
			peer.Start();

            peer.RegisterReceivedCallback(handleIncomingMessages);
        }

        private void handleIncomingMessages(object state)
        {
            var msg = peer.ReadMessage();
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.UnconnectedData:
					var request = Proto.MasterServerMessage.Parser.ParseFrom(
				        msg.SenderConnection.RemoteHailMessage.ReadBytes(msg.LengthBytes));
                    handleIncomingRequest(request, msg.SenderEndPoint);
					break;

				case NetIncomingMessageType.VerboseDebugMessage:
                    logger.Trace.Log(msg.ReadString());
                    break;
				case NetIncomingMessageType.DebugMessage:
                    logger.Debug.Log(msg.ReadString());
                    break;
                case NetIncomingMessageType.WarningMessage:
					logger.Warning.Log(msg.ReadString());
					break;
				case NetIncomingMessageType.ErrorMessage:
					logger.Error.Log(msg.ReadString());
					break;
            }
        }

        private void handleIncomingRequest(Proto.MasterServerMessage request, IPEndPoint endpoint)
        {
            switch (request.RequestCase)
            {
                case Proto.MasterServerMessage.RequestOneofCase.None:
                    logger.Warning.Log("Received incoming message without request case.");
                    break;
                case Proto.MasterServerMessage.RequestOneofCase.RegisterLobby:
                    registerLobby(request.RegisterLobby, endpoint);
                    break;
                case Proto.MasterServerMessage.RequestOneofCase.ListLobbies:
                    listLobbies(request.ListLobbies, endpoint);
                    break;
                case Proto.MasterServerMessage.RequestOneofCase.IntroduceToLobby:
                    introduceToLobby(request.IntroduceToLobby, endpoint);
                    break;
            }
        }

        private void registerLobby(Proto.RegisterLobbyRequest request, IPEndPoint endpoint)
        {
            if (lobbiesById.ContainsKey(request.Lobby.Id))
            {
                lobbiesById[request.Lobby.Id].Heartbeat();
                return;
            }

            var lobby = new Lobby(
                request.Lobby,
                new IPEndPoint(new IPAddress(request.Address.ToByteArray()), request.Port), // internal
                endpoint // external
            );

			lobbiesById.Add(request.Lobby.Id, lobby);
        }

        private void listLobbies(Proto.ListLobbiesRequest request, IPEndPoint endpoint)
        {
            foreach (var lobby in lobbiesById.Values)
            {
                var msg = peer.CreateMessage();
                msg.Write(lobby.LobbyProto.ToByteArray());
                peer.SendUnconnectedMessage(msg, endpoint);
            }
        }

        private void introduceToLobby(Proto.IntroduceToLobbyRequest request, IPEndPoint endpoint)
        {
            if (lobbiesById.TryGetValue(request.LobbyId, out var lobby))
            {
                var clientInternal = new IPEndPoint(new IPAddress(request.Address.ToByteArray()), request.Port);
				peer.Introduce(
                    lobby.InternalEndPoint,
                    lobby.ExternalEndPoint,
                    clientInternal,
                    endpoint,
                    request.Token
				);
            }
            else
            {
                logger.Error.Log($"Peer requested to connect to lobby with unknown ID: {request.LobbyId}");
            }
        }
    }
}

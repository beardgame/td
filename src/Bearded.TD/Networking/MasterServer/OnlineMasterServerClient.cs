using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bearded.TD.Meta;
using Bearded.TD.Networking.Lobby;
using Bearded.TD.Proto;
using Bearded.Utilities;
using Google.Protobuf;
using Lidgren.Network;

namespace Bearded.TD.Networking.MasterServer
{
    class OnlineMasterServerClient : IMasterServerClient
    {
        private enum MessageType : byte
        {
            Unknown = 0,
            ConnectToLobby = 1,
            ListLobbies = 2,
            RegisterLobby = 3,
            UnregisterLobby = 4,
        }

        public bool IsConnected { get; private set; }

        private readonly Logger logger;
        private readonly ClientNetworkInterface network;
        private readonly Dictionary<MessageType, Action<NetIncomingMessage>> completionActions
            = new Dictionary<MessageType, Action<NetIncomingMessage>>();

        public OnlineMasterServerClient(Logger logger, ClientInfo clientInfo)
        {
            this.logger = logger;
            network = new ClientNetworkInterface(
                logger, UserSettings.Instance.Misc.MasterServerAddress, clientInfo);
        }

        public Task<ConnectToLobbyResponse> ConnectToLobby(ConnectToLobbyRequest request)
		{
            return sendMessage<ConnectToLobbyRequest, ConnectToLobbyResponse>(request, MessageType.ConnectToLobby);
        }

        public Task<ListLobbiesResponse> ListLobbies(ListLobbiesRequest request)
        {
            return sendMessage<ListLobbiesRequest, ListLobbiesResponse>(request, MessageType.ListLobbies);
        }

        public Task<RegisterLobbyResponse> RegisterLobby(RegisterLobbyRequest request)
		{
            return sendMessage<RegisterLobbyRequest, RegisterLobbyResponse>(request, MessageType.RegisterLobby);
        }

        public Task<UnregisterLobbyResponse> UnregisterLobby(UnregisterLobbyRequest request)
		{
            return sendMessage<UnregisterLobbyRequest, UnregisterLobbyResponse>(request, MessageType.UnregisterLobby);
        }

        private Task<TResponse> sendMessage<TRequest, TResponse>(TRequest request, MessageType type)
            where TRequest : IMessage
            where TResponse : IMessage
        {
			var message = network.CreateMessage();
			message.Write((byte) type);
			message.Write(request.ToByteArray());
			network.SendMessage(message, NetworkChannel.Chat);

            var task = new TaskCompletionSource<TResponse>();

            completionActions.Add(type, (NetIncomingMessage msg) => {
                var parser = default(TResponse).Descriptor.Parser;
                var response = (TResponse) parser.ParseFrom(msg.ReadBytes(msg.LengthBytes));
                task.SetResult(response);
            });

            return task.Task;
        }

        public void Update()
        {
			foreach (var msg in network.GetMessages())
			{
				switch (msg.MessageType)
				{
                    case NetIncomingMessageType.Data:
                        handleDataMessage(msg);
						break;
                    case NetIncomingMessageType.StatusChanged:
                        handleStatusChange(msg);
                        break;
				}
			}
        }

        private void handleDataMessage(NetIncomingMessage msg)
        {
            var type = (MessageType)msg.ReadByte();
            completionActions[type](msg);
            completionActions.Remove(type);
        }

        private void handleStatusChange(NetIncomingMessage msg)
        {
            switch (msg.SenderConnection.Status)
            {
                case NetConnectionStatus.Connected:
                    IsConnected = true;
                    break;
                case NetConnectionStatus.Disconnected:
                    IsConnected = false;
                    break;
            }
        }
    }
}

using Google.Protobuf;
using Lidgren.Network;

namespace Bearded.TD.Networking.MasterServer
{
    class ServerMasterServer : MasterServer
    {
        public ServerMasterServer(NetServer server) : base(server) { }

		public void RegisterLobby(Proto.Lobby lobbyInfo)
		{
			var request = new Proto.RegisterLobbyRequest
			{
				Lobby = lobbyInfo,
				Address = ByteString.CopyFrom(NetUtility.GetMyAddress(out var mask).GetAddressBytes()),
				Port = Constants.Network.DefaultPort
			};
			var protoMsg = CreateMessage();
			protoMsg.RegisterLobby = request;
			SendMessage(protoMsg);
		}
    }
}

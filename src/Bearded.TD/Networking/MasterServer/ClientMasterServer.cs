using Google.Protobuf;
using Lidgren.Network;

namespace Bearded.TD.Networking.MasterServer;

class ClientMasterServer : MasterServer
{
	public ClientMasterServer(NetClient client) : base(client) { }

	public void ListLobbies()
	{
		var request = new Proto.ListLobbiesRequest();
		var protoMsg = CreateMessage();
		protoMsg.ListLobbies = request;
		SendMessage(protoMsg);
	}

	public void ConnectToLobby(long lobbyId)
	{
		var request = new Proto.IntroduceToLobbyRequest
		{
			LobbyId = lobbyId,
			Token = "can i haz td lobby pls",
			Address = ByteString.CopyFrom(NetUtility.GetMyAddress(out _).GetAddressBytes()),
			Port = Constants.Network.DefaultPort
		};
		var protoMsg = CreateMessage();
		protoMsg.IntroduceToLobby = request;
		SendMessage(protoMsg);
	}
}
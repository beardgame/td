using System.Net;

namespace Bearded.TD.MasterServer
{
    class Lobby
    {
        public Proto.Lobby LobbyProto { get; }
        public IPEndPoint InternalEndPoint { get; }
        public IPEndPoint ExternalEndPoint { get; }

        public Lobby(Proto.Lobby lobbyProto, IPEndPoint internalEndPoint, IPEndPoint externalEndPoint)
        {
			LobbyProto = lobbyProto;
			InternalEndPoint = internalEndPoint;
			ExternalEndPoint = externalEndPoint;
        }

        public void Heartbeat()
        {
            // Boing.
        }
    }
}

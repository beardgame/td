using System;
using System.Net;

namespace Bearded.TD.MasterServer
{
    sealed class Lobby
    {
        public Proto.Lobby LobbyProto { get; }
        public IPEndPoint InternalEndPoint { get; }
        public IPEndPoint ExternalEndPoint { get; }

        private long lastHeartBeat;

        public long AgeInSeconds => DateTimeOffset.Now.ToUnixTimeSeconds() - lastHeartBeat;

        public Lobby(Proto.Lobby lobbyProto, IPEndPoint internalEndPoint, IPEndPoint externalEndPoint)
        {
			LobbyProto = lobbyProto;
			InternalEndPoint = internalEndPoint;
			ExternalEndPoint = externalEndPoint;
            Heartbeat();
        }

        public void Heartbeat()
        {
            lastHeartBeat = DateTimeOffset.Now.ToUnixTimeSeconds();
        }
    }
}

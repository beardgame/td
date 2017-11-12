using Bearded.TD.Networking.Serialization;
using Lidgren.Network;

namespace Bearded.TD.Networking
{
    struct ClientInfo
    {
        private string playerName;

        public string PlayerName => playerName;
        // API version
        // Hash of gamedata files

        public ClientInfo(string playerName)
        {
            this.playerName = playerName;
        }

        public void Serialize(INetBufferStream buffer)
        {
            buffer.Serialize(ref playerName);
        }

        public static ClientInfo FromBuffer(NetBuffer buffer)
        {
            var clientInfo = new ClientInfo();
            var reader = new NetBufferReader(buffer);
            clientInfo.Serialize(reader);
            return clientInfo;
        }
    }
}
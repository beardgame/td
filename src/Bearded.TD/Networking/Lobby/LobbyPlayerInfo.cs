using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking.Lobby
{
    struct LobbyPlayerInfo
    {
        private Id<Player> id;

        public Id<Player> Id => id;

        public LobbyPlayerInfo(Id<Player> id)
        {
            this.id = id;
        }

        public void Serialize(INetBufferStream buffer)
        {
            buffer.Serialize(ref id);
        }

        public static LobbyPlayerInfo ForPlayer(Player player)
        {
            return new LobbyPlayerInfo(player.Id);
        }

        public static LobbyPlayerInfo FromBuffer(NetBuffer buffer)
        {
            var playerInfo = new LobbyPlayerInfo();
            var reader = new NetBufferReader(buffer);
            playerInfo.Serialize(reader);
            return playerInfo;
        }
    }
}
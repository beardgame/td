using amulware.Graphics;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking.Lobby
{
    struct LobbyPlayerInfo
    {
        private Id<Player> id;
        private Color color;

        public Id<Player> Id => id;
        public Color Color => color;

        public LobbyPlayerInfo(Id<Player> id, Color color)
        {
            this.id = id;
            this.color = color;
        }

        public void Serialize(INetBufferStream buffer)
        {
            buffer.Serialize(ref id);
            buffer.Serialize(ref color);
        }

        public static LobbyPlayerInfo ForPlayer(Player player)
        {
            return new LobbyPlayerInfo(player.Id, player.Color);
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
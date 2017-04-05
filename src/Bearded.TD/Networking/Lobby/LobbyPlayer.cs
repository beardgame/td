using Bearded.TD.Game.Players;

namespace Bearded.TD.Networking.Lobby
{
    class LobbyPlayer
    {
        public Player Player { get; }
        public bool IsReady { get; set; }

        public LobbyPlayer(Player player)
        {
            Player = player;
        }
    }
}

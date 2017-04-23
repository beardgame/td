using Bearded.TD.Game.Players;

namespace Bearded.TD.Networking.Lobby
{
    class LobbyPlayer
    {
        public Player Player { get; }
        public LobbyPlayerState State { get; set; } = LobbyPlayerState.Unknown;

        public LobbyPlayer(Player player)
        {
            Player = player;
        }
    }

    public enum LobbyPlayerState : byte
    {
        Unknown = 0,
        Connecting = 1,
        Waiting = 2,
        Ready = 3,
    }
}

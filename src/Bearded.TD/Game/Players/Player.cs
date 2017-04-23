using amulware.Graphics;
using Bearded.TD.Networking.Lobby;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Players
{
    sealed class Player : IIdable<Player>
    {
        public Id<Player> Id { get; }
        public string Name { get; }
        public Color Color { get; }
        public PlayerConnectionState ConnectionState { get; set; }

        public Player(Id<Player> id, string name, Color color)
        {
            Id = id;
            Name = name;
            Color = color;
            ConnectionState = PlayerConnectionState.Unknown;
        }
    }
}
using Bearded.TD.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    static class CreateGameState
    {
        public static ICommand Command(GameInstance game, int radius)
            => new Implementation(game, radius);

        private class Implementation : ICommand
        {
            private readonly GameInstance game;
            private readonly int radius;

            public Implementation(GameInstance game, int radius)
            {
                this.game = game;
                this.radius = radius;
            }

            public void Execute()
            {
                var tilemap = new Tilemap<TileInfo>(radius, _ => new TileInfo(Directions.None, TileInfo.Type.Unknown));
                var state = new GameState(game.Meta, new Level(tilemap));
                game.InitialiseState(state);
            }

            public ICommandSerializer Serializer => new Serializer(radius);
        }

        private class Serializer : ICommandSerializer
        {
            private int radius;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(int radius)
            {
                this.radius = radius;
            }

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game, radius);

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref radius);
            }
        }
    }
}

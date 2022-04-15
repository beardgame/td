using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Loading;

static class TurnCrevicesIntoFluidSinks
{
    public static ISerializableCommand<GameInstance> Command(GameInstance game) => new Implementation(game);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameInstance game;

        public Implementation(GameInstance game)
        {
            this.game = game;
        }

        public void Execute()
        {
            game.MustBeLoading();

            var geometry = game.State.GeometryLayer;
            var fluids = game.State.FluidLayer;

            foreach (var tile in Tilemap.EnumerateTilemapWith(game.State.Level.Radius))
            {
                var info = geometry[tile];

                if (info.Type != TileType.Crevice)
                    continue;

                fluids.Water.AddSink(tile);
            }
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer();
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        [UsedImplicitly]
        public Serializer()
        {
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game) => new Implementation(game);

        public void Serialize(INetBufferStream stream)
        {
        }
    }
}
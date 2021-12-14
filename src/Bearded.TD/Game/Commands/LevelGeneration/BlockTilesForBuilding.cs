using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.LevelGeneration;

static class BlockTilesForBuilding
{
    public static ISerializableCommand<GameInstance> Command(GameInstance game, IList<Tile> tiles)
        => new Implementation(game, tiles);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameInstance game;
        private readonly IList<Tile> tiles;

        public Implementation(GameInstance game, IList<Tile> tiles)
        {
            this.game = game;
            this.tiles = tiles;
        }

        public void Execute()
        {
            game.MustBeLoading();

            tiles.ForEach(game.State.BuildingPlacementLayer.BlockTileForBuilding);
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(tiles);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private (int x, int y)[] tiles = {};

        [UsedImplicitly]
        public Serializer() { }

        public Serializer(IEnumerable<Tile> tiles)
        {
            this.tiles = tiles.Select(tile => (tile.X, tile.Y)).ToArray();
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            => new Implementation(
                game,
                tiles.Select(coords => new Tile(coords.x, coords.y)).ToList());

        public void Serialize(INetBufferStream stream)
        {
            stream.SerializeArrayCount(ref tiles);
            for (var i = 0; i < tiles.Length; i++)
            {
                stream.Serialize(ref tiles[i].x);
                stream.Serialize(ref tiles[i].y);
            }
        }
    }
}
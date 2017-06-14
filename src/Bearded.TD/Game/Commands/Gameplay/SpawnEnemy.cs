using Bearded.TD.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class SpawnEnemy
    {
        public static ICommand Command(GameState game, Tile<TileInfo> tile, UnitBlueprint blueprint)
            => new Implementation(game, tile, blueprint);

        private class Implementation : ICommand
        {
            private readonly GameState game;
            private readonly Tile<TileInfo> tile;
            private readonly UnitBlueprint blueprint;

            public Implementation(GameState game, Tile<TileInfo> tile, UnitBlueprint blueprint)
            {
                this.game = game;
                this.tile = tile;
                this.blueprint = blueprint;
            }

            public void Execute() => game.Add(new EnemyUnit(blueprint, tile));

            public ICommandSerializer Serializer => new Serializer(blueprint, tile);
        }

        private class Serializer : ICommandSerializer
        {
            private Id<UnitBlueprint> blueprint;
            private int x;
            private int y;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(UnitBlueprint blueprint, Tile<TileInfo> tile)
            {
                this.blueprint = blueprint.Id;
                x = tile.X;
                y = tile.Y;
            }

            public ICommand GetCommand(GameInstance game) => new Implementation(
                game.State, new Tile<TileInfo>(game.State.Level.Tilemap, x, y), game.Blueprints.Units[blueprint]);

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref blueprint);
                stream.Serialize(ref x);
                stream.Serialize(ref y);
            }
        }
    }
}
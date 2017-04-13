using Bearded.TD.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    class SpawnEnemy : ICommand
    {
        public static ICommand Command(GameState game, Tile<TileInfo> tile, UnitBlueprint blueprint)
            => new SpawnEnemy(game, tile, blueprint);

        private readonly GameState game;
        private readonly Tile<TileInfo> tile;
        private readonly UnitBlueprint blueprint;

        private SpawnEnemy(GameState game, Tile<TileInfo> tile, UnitBlueprint blueprint)
        {
            this.game = game;
            this.tile = tile;
            this.blueprint = blueprint;
        }
        
        public void Execute() => game.Add(new EnemyUnit(blueprint, tile));
        public ICommandSerializer Serializer { get; }
    }
}
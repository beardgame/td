using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation
{
    sealed class EmptyTilemapGenerator : ITilemapGenerator
    {
        public Tilemap<TileGeometry> Generate(LevelGenerationParameters parameters, int seed)
        {
            return new(parameters.Radius, _ => new TileGeometry(TileType.Floor, 1, Unit.Zero));
        }
    }
}

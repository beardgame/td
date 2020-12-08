using Bearded.TD.Game.GameState.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation
{
    sealed class EmptyTilemapGenerator : ITilemapGenerator
    {
        public Tilemap<TileGeometry> Generate(int radius, int seed)
        {
            return new Tilemap<TileGeometry>(radius, _ => new TileGeometry(TileType.Floor, 1, Unit.Zero));
        }
    }
}

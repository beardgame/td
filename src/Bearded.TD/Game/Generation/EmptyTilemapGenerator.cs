using Bearded.TD.Game.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation
{
    sealed class EmptyTilemapGenerator : ITilemapGenerator
    {
        public Tilemap<TileGeometry> Generate(int radius, int seed)
        {
            return new Tilemap<TileGeometry>(radius, _ => new TileGeometry(TileType.Floor, 1));
        }
    }
}

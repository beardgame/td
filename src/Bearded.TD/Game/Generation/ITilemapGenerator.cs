using Bearded.TD.Game.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation
{
    interface ITilemapGenerator
    {
        Tilemap<TileGeometry> Generate(int radius, int seed);
    }
}

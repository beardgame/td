using Bearded.TD.Game.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation
{
    interface ITilemapGenerator
    {
        Tilemap<TileInfo.Type> Generate(int radius);
    }
}

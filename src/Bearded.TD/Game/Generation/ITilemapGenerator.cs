using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;

namespace Bearded.TD.Game.Generation
{
    interface ITilemapGenerator
    {
        void Fill(Tilemap<TileInfo> tilemap);
    }
}
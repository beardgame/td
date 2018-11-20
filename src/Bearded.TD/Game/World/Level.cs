using Bearded.TD.Tiles;

namespace Bearded.TD.Game.World
{
    sealed class Level : Level<TileInfo>
    {
        public Level(Tilemap<TileInfo> tilemap)
            : base(tilemap)
        {
        }
    }
}

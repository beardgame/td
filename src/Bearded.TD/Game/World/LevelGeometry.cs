using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.World
{
    class LevelGeometry
    {
        public delegate void TilePassibilityChangeEventHandler(Tile<TileInfo> tile);
        public event TilePassibilityChangeEventHandler TilePassabalityChanged;

        public Tilemap<TileInfo> Tilemap { get; }

        public LevelGeometry(Tilemap<TileInfo> tilemap)
        {
            Tilemap = tilemap;
        }

        public void ToggleTileType(Tile<TileInfo> tile)
        {
            if (!tile.IsValid) throw new System.ArgumentOutOfRangeException();
            var info = Tilemap[tile];
            info.SetTileType(info.TileType == TileInfo.Type.Wall
                ? TileInfo.Type.Floor
                : TileInfo.Type.Wall);
        }

        public void UpdatePassability(Tile<TileInfo> tile, TileInfo info = null)
        {
            if (info == null)
                info = tile.Info;

            foreach (var dir in Tiles.Tilemap.Directions)
            {
                var neighbour = tile.Neighbour(dir);
                if (!neighbour.IsValid) continue;
                if (info.IsPassable)
                    neighbour.Info.OpenTo(dir.Opposite());
                else
                    neighbour.Info.CloseTo(dir.Opposite());
            }

            TilePassabalityChanged?.Invoke(tile);
        }
    }
}

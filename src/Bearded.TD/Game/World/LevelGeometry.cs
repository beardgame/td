using Bearded.TD.Game.Buildings;
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

            updatePassability(tile);
        }

        public void SetBuilding(Tile<TileInfo> tile, Building building)
        {
            if (!tile.IsValid) throw new System.ArgumentOutOfRangeException();
            tile.Info.SetBuilding(building);
            updatePassability(tile);
        }

        private void updatePassability(Tile<TileInfo> tile)
        {
            var isPassable = tile.Info.IsPassable;

            foreach (var dir in tile.Info.ValidDirections.Enumerate())
            {
                var neighbour = tile.Neighbour(dir);
                if (isPassable)
                    neighbour.Info.OpenTo(dir.Opposite());
                else
                    neighbour.Info.CloseTo(dir.Opposite());
            }

            TilePassabalityChanged?.Invoke(tile);
        }
    }
}

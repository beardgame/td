using Bearded.TD.Game.Buildings;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.World
{
    class LevelGeometry
    {
        public delegate void TilePassibilityChangeEventHandler(Tile<TileInfo> tile);
        public event TilePassibilityChangeEventHandler TilePassabilityChanged;

        public Tilemap<TileInfo> Tilemap { get; }

        public LevelGeometry(Tilemap<TileInfo> tilemap)
        {
            Tilemap = tilemap;
        }

        public void Initialise()
        {
            Tilemap.ForEach(updatePassability);
        }

        public void SetTileType(Tile<TileInfo> tile, TileInfo.Type type, TileDrawInfo drawInfo)
        {
            if (!tile.IsValid) throw new System.ArgumentOutOfRangeException();

            var tileInfo = tile.Info;

            tileInfo.SetTileType(type);
            tileInfo.SetDrawInfo(drawInfo);

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

            TilePassabilityChanged?.Invoke(tile);
        }
    }
}

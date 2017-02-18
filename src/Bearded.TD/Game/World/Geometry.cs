using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.World
{
    class Geometry
    {
        public delegate void TilePassibilityChangeEventHandler(Tile<TileInfo> tile);
        public event TilePassibilityChangeEventHandler TilePassabalityChanged;

        public Tilemap<TileInfo> Tilemap { get; }

        public Geometry(Tilemap<TileInfo> tilemap)
        {
            Tilemap = tilemap;
        }

        public void SetPassability(Tile<TileInfo> tile, bool passable)
        {
            if (!tile.IsValid) throw new System.ArgumentOutOfRangeException();
            var info = Tilemap[tile];
            if (info.IsPassable == passable) return;

            info.TogglePassability();

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

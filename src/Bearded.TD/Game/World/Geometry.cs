using Bearded.TD.Game.Tilemap;

namespace Bearded.TD.Game.World
{
    class Geometry
    {
        private readonly Tilemap<TileInfo> tilemap;

        public Geometry(Tilemap<TileInfo> tilemap)
        {
            this.tilemap = tilemap;
        }

        public void SetPassability(Tile<TileInfo> tile, bool passable)
        {
            if (!tile.IsValid) throw new System.ArgumentOutOfRangeException();
            var info = tilemap[tile];
            if (info.IsPassable == passable) return;

            info.TogglePassability();
            foreach (var dir in Tilemap.Tilemap.Directions)
            {
                var neighbour = tile.Neighbour(dir);
                if (!neighbour.IsValid) continue;
                if (info.IsPassable)
                    neighbour.Info.OpenTo(dir.Opposite());
                else
                    neighbour.Info.CloseTo(dir.Opposite());
            }
        }
    }
}

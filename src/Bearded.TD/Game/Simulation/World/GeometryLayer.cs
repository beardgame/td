using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.World
{
    sealed class GeometryLayer
    {
        private readonly GlobalGameEvents events;
        private Tilemap<DrawableTileGeometry> tilemap { get; }

        public GeometryLayer(GlobalGameEvents events, int radius)
        {
            this.events = events;
            tilemap = new Tilemap<DrawableTileGeometry>(radius);
        }

        public void SetTileGeometry(Tile tile, TileGeometry geometry, TileDrawInfo drawInfo)
        {
            if (!tilemap.IsValidTile(tile)) throw new System.ArgumentOutOfRangeException();

            tilemap[tile] = new DrawableTileGeometry(geometry, drawInfo);
            
            onDrawInfoChanged(tile);
            onTileTypeChanged(tile, geometry.Type);
        }

        public void SetDrawInfo(Tile tile, TileDrawInfo drawInfo)
        {
            if (!tilemap.IsValidTile(tile)) throw new System.ArgumentOutOfRangeException();

            tilemap[tile] = tilemap[tile].WithDrawInfo(drawInfo);

            onDrawInfoChanged(tile);
        }

        private void onTileTypeChanged(Tile tile, TileType type)
        {
            events.Send(new TileTypeChanged(tile, type));
        }

        private void onDrawInfoChanged(Tile tile)
        {
            events.Send(new TileDrawInfoChanged(tile));
        }

        public DrawableTileGeometry this[Tile tile] => tilemap[tile];
    }
}

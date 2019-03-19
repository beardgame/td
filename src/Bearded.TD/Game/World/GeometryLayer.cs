using Bearded.TD.Game.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.World
{
    sealed class GeometryLayer
    {
        private readonly GameEvents events;
        private Tilemap<TileGeometry> tilemap { get; }

        public GeometryLayer(GameEvents events, int radius)
        {
            this.events = events;
            tilemap = new Tilemap<TileGeometry>(radius);
        }

        public void SetTileType(Tile tile, TileType type, TileDrawInfo drawInfo)
        {
            if (!tilemap.IsValidTile(tile)) throw new System.ArgumentOutOfRangeException();

            tilemap[tile] = new TileGeometry(type, drawInfo);
            
            onDrawInfoChanged(tile);
            onTileTypeChanged(tile, type);
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

        public TileGeometry this[Tile tile] => tilemap[tile];
    }
}

using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.World
{
    class GeometryLayer
    {
        private readonly GameEvents events;
        private Tilemap<TileGeometry> tilemap { get; }

        public GeometryLayer(GameEvents events, Tilemap<TileGeometry> tilemap)
        {
            this.events = events;
            this.tilemap = tilemap;
        }

        public void Initialise()
        {
            tilemap.ForEach(updatePassability);
        }

        public void SetTileType(Tile tile, TileGeometry.TileType type, TileDrawInfo drawInfo)
        {
            if (tilemap.IsValidTile(tile)) throw new System.ArgumentOutOfRangeException();

            tilemap[tile] = new TileGeometry(type, drawInfo);
            
            onDrawInfoChanged(tile);
            onTileTypeChanged(tile);
            updatePassability(tile);
        }

        public void SetDrawInfo(Tile tile, TileDrawInfo drawInfo)
        {
            if (tilemap.IsValidTile(tile)) throw new System.ArgumentOutOfRangeException();

            tilemap[tile] = tilemap[tile].WithDrawInfo(drawInfo);

            onDrawInfoChanged(tile);
        }

        private void onTileTypeChanged(Tile tile)
        {
            events.Send(new TileTypeChanged(tile));
        }

        private void onDrawInfoChanged(Tile tile)
        {
            events.Send(new TileDrawInfoChanged(tile));
        }

        public TileGeometry this[Tile tile] => tilemap[tile];

        public void SetBuilding(Tile tile, Building building)
        {
            if (!tile.IsValid) throw new System.ArgumentOutOfRangeException();
            tile.Info.FinishedBuilding = building;
            updatePassability(tile);
        }

        private void updatePassability(Tile tile)
        {
            var isPassable = tile.Info.IsPassableFor(TileInfo.PassabilityLayer.Unit);

            foreach (var dir in tile.Info.ValidDirections.Enumerate())
            {
                var neighbour = tile.Neighbour(dir);
                if (isPassable)
                    neighbour.Info.OpenForUnitsTo(dir.Opposite());
                else
                    neighbour.Info.CloseForUnitsTo(dir.Opposite());
            }

            events.Send(new TilePassabilityChanged(tile));
        }
    }
}

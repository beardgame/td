using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.World
{
    class LevelGeometry
    {
        private readonly GameEvents events;
        public Tilemap<TileInfo> Tilemap { get; }

        public LevelGeometry(GameEvents events, Tilemap<TileInfo> tilemap)
        {
            this.events = events;
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

            events.Send(new TileDrawInfoChanged(tile));

            updatePassability(tile);
        }

        public void SetBuilding(Tile<TileInfo> tile, Building building)
        {
            if (!tile.IsValid) throw new System.ArgumentOutOfRangeException();
            tile.Info.FinishedBuilding = building;
            updatePassability(tile);
        }

        private void updatePassability(Tile<TileInfo> tile)
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

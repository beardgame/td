using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Buildings
{
    sealed class BuildingPlacementLayer
    {
        private readonly GeometryLayer geometryLayer;
        private readonly BuildingLayer buildingLayer;
        private readonly HashSet<Tile> blockedTiles = new HashSet<Tile>();

        public BuildingPlacementLayer(GeometryLayer geometryLayer, BuildingLayer buildingLayer)
        {
            this.geometryLayer = geometryLayer;
            this.buildingLayer = buildingLayer;
        }

        public void BlockTileForBuilding(Tile tile) => blockedTiles.Add(tile);

        public bool IsTileValidForBuilding(Tile tile)
        {
            return tile.IsValid
                && !blockedTiles.Contains(tile)
                && geometryLayer[tile].Type == TileGeometry.TileType.Floor
                && buildingLayer[tile] == null;
        }

        public bool AreTilesValidForBuilding(IEnumerable<Tile> tiles) => tiles.All(IsTileValidForBuilding);
    }
}

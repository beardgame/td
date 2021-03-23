using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Selection
{
    sealed class SelectionLayer
    {
        private readonly BuildingLayer buildingLayer;

        public SelectionLayer(BuildingLayer buildingLayer)
        {
            this.buildingLayer = buildingLayer;
        }

        public ISelectable? SelectableForTile(Tile tile)
        {
            var building = buildingLayer.GetBuildingFor(tile);
            if (building != null)
            {
                return building;
            }

            // TODO: add workers and enemies
            return null;
        }
    }
}

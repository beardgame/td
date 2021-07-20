using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Buildings
{
    abstract class BuildingFootprintBase<T> : Component<T>, IBuildingFootprint
    {
        public abstract IEnumerable<Tile> OccupiedTiles { get; }

        protected override void Initialize()
        {
            foreach (var occupiedTile in OccupiedTiles)
            {
                Events.Send(new TileEntered(occupiedTile));
            }
        }
    }
}

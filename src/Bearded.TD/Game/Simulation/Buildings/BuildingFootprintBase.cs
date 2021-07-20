using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Buildings
{
    abstract class BuildingFootprintBase<T> : Component<T>, IBuildingFootprint, IListener<AccumulateOccupiedTiles>
    {
        protected override void Initialize()
        {
            Events.Subscribe(this);
            foreach (var occupiedTile in GetOccupiedTiles())
            {
                Events.Send(new TileEntered(occupiedTile));
            }
        }

        public void HandleEvent(AccumulateOccupiedTiles @event)
        {
            @event.AddTiles(GetOccupiedTiles());
        }

        protected abstract IEnumerable<Tile> GetOccupiedTiles();
    }
}

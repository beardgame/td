using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Footprints
{
    abstract class TileOccupationBase<T> : Component<T>, ITileOccupation
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

using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Footprints;

abstract class TileOccupationBase : Component, ITileOccupation
{
    public abstract IEnumerable<Tile> OccupiedTiles { get; }

    protected override void OnAdded()
    {
        foreach (var occupiedTile in OccupiedTiles)
        {
            Events.Send(new TileEntered(occupiedTile));
        }
    }
}

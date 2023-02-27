using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Footprints;

abstract class TileNotifierBase : Component
{
    protected abstract IEnumerable<Tile> OccupiedTiles { get; }

    protected override void OnAdded()
    {
        // TODO: the fact that we are sending events in OnAdded is icky, and only works because the tile presence
        //       was already added before. The check before is an ugly way to force this for now, but we should really
        //       consider whether this can be solved more neatly.
        State.Satisfies(Owner.TryGetSingleComponent<ITilePresence>(out _),
            "Attempted to notify tile additions before tile presence added.");
        foreach (var occupiedTile in OccupiedTiles)
        {
            Events.Send(new TileEntered(occupiedTile));
        }
    }
}

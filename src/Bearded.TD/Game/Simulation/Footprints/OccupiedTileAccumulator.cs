using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Footprints;

static class OccupiedTileAccumulator
{
    public static ImmutableArray<Tile> AccumulateOccupiedTiles(IComponentOwner obj)
    {
        var hashSet = new HashSet<Tile>();
        obj.GetComponents<ITileOccupation>().Select(b => b.OccupiedTiles).ForEach(hashSet.UnionWith);
        return hashSet.ToImmutableArray();
    }
}

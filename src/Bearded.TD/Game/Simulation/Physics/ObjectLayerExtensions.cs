using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Physics;

static class ObjectLayerExtensions
{
    public static ImmutableArray<GameObject> EnumerateAllObjects(this ObjectLayer layer, Level level)
    {
        return Tilemap.EnumerateTilemapWith(level.Radius)
            .SelectMany(layer.GetObjectsOnTile)
            .Distinct()
            .ToImmutableArray();
    }
}

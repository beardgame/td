using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Footprints;

static class TilePresenceExtensions
{
    public static ITilePresence GetTilePresence(this GameObject obj)
    {
        return obj.GetComponents<ITilePresence>().Single();
    }

    public static bool TryGetTilePresence(this GameObject obj, [NotNullWhen(true)] out ITilePresence? presence)
    {
        presence = obj.GetComponents<ITilePresence>().SingleOrDefault();
        return presence is not null;
    }
}

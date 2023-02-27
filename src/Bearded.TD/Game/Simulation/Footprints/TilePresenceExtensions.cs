using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Footprints;

static class TilePresenceExtensions
{
    public static ITilePresence GetTilePresence(this GameObject obj)
    {
        return obj.GetComponents<ITilePresence>().Single();
    }
}

using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Zones;

namespace Bearded.TD.Game.Simulation.Exploration;

readonly struct ZoneRevealed : IGlobalEvent
{
    public Zone Zone { get; }

    public ZoneRevealed(Zone zone)
    {
        Zone = zone;
    }
}
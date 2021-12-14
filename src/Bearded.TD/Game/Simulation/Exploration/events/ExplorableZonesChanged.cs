using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Zones;

namespace Bearded.TD.Game.Simulation.Exploration;

readonly struct ExplorableZonesChanged : IGlobalEvent
{
    public ImmutableArray<Zone> ExplorableZones { get; }

    public ExplorableZonesChanged(ImmutableArray<Zone> explorableZones)
    {
        ExplorableZones = explorableZones;
    }
}
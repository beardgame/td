using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Zones;

namespace Bearded.TD.Game.Simulation.Exploration;

record struct ExplorableZonesChanged(Faction Faction, ImmutableArray<Zone> ExplorableZones) : IGlobalEvent;

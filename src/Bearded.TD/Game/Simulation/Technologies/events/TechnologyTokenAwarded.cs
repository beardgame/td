using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Technologies;

record struct TechnologyTokenAwarded(FactionTechnology FactionTechnology) : IGlobalEvent;

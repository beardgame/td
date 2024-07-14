using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Technologies;

readonly record struct TechnologyUnlocked(FactionTechnology FactionTechnology, ITechnologyBlueprint Technology)
    : IGlobalEvent;

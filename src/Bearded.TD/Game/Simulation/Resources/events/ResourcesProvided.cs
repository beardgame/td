using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Resources;

readonly record struct ResourcesProvided(FactionResources Resources, ResourceAmount AmountProvided) : IGlobalEvent;

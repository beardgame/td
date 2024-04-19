using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Resources;

readonly record struct ResourcesConsumed(FactionResources Resources, ResourceAmount AmountConsumed) : IGlobalEvent;

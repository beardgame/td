using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Resources;

readonly record struct ResourcesConsumed<T>(FactionResources Resources, Resource<T> AmountConsumed) : IGlobalEvent
    where T : IResourceType;

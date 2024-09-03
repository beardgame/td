using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Resources;

readonly record struct ResourcesChanged<T>(FactionResources Resources, Resource<T> NewAmount) : IGlobalEvent
    where T : IResourceType;

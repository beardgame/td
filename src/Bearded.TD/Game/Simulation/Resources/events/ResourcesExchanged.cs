using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Resources;

readonly record struct ResourcesExchanged<TFrom, TTo>(
    FactionResources FactionResources, Resource<TFrom> From, Resource<TTo> To)
    : IGlobalEvent
    where TFrom : IResourceType where TTo : IResourceType;

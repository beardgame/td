using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Resources;

readonly record struct ResourcesProvidedPreview<T>(FactionResources Resources, Resource<T> AmountProvided) : IGlobalPreviewEvent
    where T : IResourceType;

readonly record struct ResourcesProvided<T>(FactionResources Resources, Resource<T> AmountProvided) : IGlobalEvent
    where T : IResourceType;

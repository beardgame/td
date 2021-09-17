using Bearded.TD.Shared.Events;

namespace Bearded.TD.Generators.Tests.Listeners
{
    namespace EventImplementationsNamespace
    {
        public readonly struct FirstEvent : IEvent {}
        public readonly struct SecondEvent : IEvent {}
        public readonly struct PreviewEvent : IPreviewEvent {}
    }
}

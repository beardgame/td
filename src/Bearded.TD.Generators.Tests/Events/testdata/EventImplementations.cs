using Bearded.TD.Shared.Events;

namespace Bearded.TD.Generators.Tests.Events
{
    namespace EventImplementationsNamespace
    {
        public readonly struct FirstEvent : IEvent {}
        public readonly struct SecondEvent : IEvent {}
        public readonly struct PreviewEvent : IPreviewEvent {}
    }
}

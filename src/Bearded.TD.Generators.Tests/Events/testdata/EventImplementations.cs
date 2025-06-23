using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Generators.Tests.Events
{
    namespace EventImplementationsNamespace
    {
        public readonly struct FirstEvent : IComponentEvent;
        public readonly struct SecondEvent : IComponentEvent;
        public readonly struct PreviewEvent : IComponentPreviewEvent;
    }
}

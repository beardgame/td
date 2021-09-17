using Bearded.TD.Generators.Tests.Listeners.EventImplementationsNamespace;
using Bearded.TD.Generators.Tests.Listeners.GameEventsNamespace;
using Bearded.TD.Shared.Events;
using JetBrains.Annotations;

namespace Bearded.TD.Generators.Tests.Listeners
{
    namespace ListenerNamespace
    {
        [EventListener(typeof(MyEvents))]
        [UsedImplicitly]
        // ReSharper disable once PartialTypeWithSinglePart
        sealed partial class MyEventListener
            : IListener<FirstEvent>, IListener<SecondEvent>, IPreviewListener<PreviewEvent>
        {
            public void HandleEvent(FirstEvent @event) { }
            public void HandleEvent(SecondEvent @event) { }
            public void PreviewEvent(ref PreviewEvent @event) { }
        }
    }
}

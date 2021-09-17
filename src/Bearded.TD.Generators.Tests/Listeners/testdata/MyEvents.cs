using Bearded.TD.Shared.Events;

namespace Bearded.TD.Generators.Tests.Listeners
{
    namespace GameEventsNamespace
    {
        sealed class MyEvents : GameEvents<IEvent, IPreviewEvent> {}
    }
}

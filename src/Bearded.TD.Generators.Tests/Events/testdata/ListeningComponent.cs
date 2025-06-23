using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Generators.Tests.Events.EventImplementationsNamespace;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Generators.Tests.Events;

sealed partial class ListeningComponent : Component
{
    [Handler]
    private void onFirstEvent(FirstEvent e) { }
}

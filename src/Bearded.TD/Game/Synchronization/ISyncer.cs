using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities;

namespace Bearded.TD.Game.Synchronization;

interface ISyncer
{
    Id<GameObject> GameObjectId { get; }
    IStateToSync GetCurrentStateToSync();
}

using Bearded.Utilities;

namespace Bearded.TD.Game.Synchronization;

interface ISyncer<T>
{
    Id<T> EntityId { get; }
    IStateToSync GetCurrentStateToSync();
}
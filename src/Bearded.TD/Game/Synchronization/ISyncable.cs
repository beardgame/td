namespace Bearded.TD.Game.Synchronization
{
    interface ISyncable
    {
        IStateToSync GetCurrentStateToSync();
    }
}

namespace Bearded.TD.Game.Synchronization
{
    interface ISyncable<TState>
    {
        TState GetCurrentState();

        void SyncFrom(TState state);
    }
}
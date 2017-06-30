namespace Bearded.TD.Utilities
{
    interface ISyncable<TState>
    {
        TState GetCurrentState();

        void SyncFrom(TState state);
    }
}
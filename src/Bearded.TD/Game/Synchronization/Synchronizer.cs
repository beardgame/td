using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Synchronization
{
    interface IStateToSync
    {
        void Serialize(INetBufferStream stream);
        void Apply();
    }

    sealed class StateToSync<TSubject> : IStateToSync where TSubject : ISyncable
    {
        private readonly TSubject subject;
        private readonly ISynchronizedState<TSubject> state;

        public StateToSync(TSubject subject, ISynchronizedState<TSubject> state)
        {
            this.subject = subject;
            this.state = state;
        }

        public void Serialize(INetBufferStream stream)
        {
            state.Serialize(stream);
        }

        public void Apply()
        {
            state.ApplyTo(subject);
        }
    }
}

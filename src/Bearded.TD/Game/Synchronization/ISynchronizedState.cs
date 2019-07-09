using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Synchronization
{
    interface ISynchronizedState<in TSubject>
    {
        void ApplyTo(TSubject subject);
        void Serialize(INetBufferStream stream);
    }
}

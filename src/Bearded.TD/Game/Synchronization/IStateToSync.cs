using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Synchronization;

interface IStateToSync
{
    void Serialize(INetBufferStream stream);
    void Apply();
}

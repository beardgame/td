using Bearded.TD.Commands;

namespace Bearded.TD.Networking.Serialization
{
    interface ICommandSerializer<TObject>
    {
        ISerializableCommand<TObject> GetCommand(TObject game);
        void Serialize(INetBufferStream stream);
    }
}
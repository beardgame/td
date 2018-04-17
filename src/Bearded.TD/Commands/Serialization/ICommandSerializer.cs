using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands.Serialization
{
    interface ICommandSerializer<TObject>
    {
        ISerializableCommand<TObject> GetCommand(TObject game);
        void Serialize(INetBufferStream stream);
    }
}
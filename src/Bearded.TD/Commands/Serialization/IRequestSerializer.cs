using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands.Serialization
{
    interface IRequestSerializer<TObject>
    {
        IRequest<TObject> GetRequest(TObject game);
        void Serialize(INetBufferStream stream);
    }
}
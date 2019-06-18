using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands.Serialization
{
    interface IRequestSerializer<TActor, TObject>
    {
        IRequest<TActor, TObject> GetRequest(TObject game);
        void Serialize(INetBufferStream stream);
    }
}

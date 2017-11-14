using Bearded.TD.Commands;

namespace Bearded.TD.Networking.Serialization
{
    interface IRequestSerializer<in TContext, in TSender>
    {
        IRequest<TContext, TSender> GetRequest(TContext game, TSender sender);
        void Serialize(INetBufferStream stream);
    }
}
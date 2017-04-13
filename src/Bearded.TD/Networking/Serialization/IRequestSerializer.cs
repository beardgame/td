using Bearded.TD.Commands;
using Bearded.TD.Game;

namespace Bearded.TD.Networking.Serialization
{
    interface IRequestSerializer
    {
        IRequest GetRequest(GameInstance game);
        void Serialize(INetBufferStream stream);
    }
}
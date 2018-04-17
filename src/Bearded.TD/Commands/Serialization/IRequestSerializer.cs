using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands.Serialization
{
    interface IRequestSerializer<TObject>
    {
        IRequest<TObject> GetRequest(TObject game, Player sender);
        void Serialize(INetBufferStream stream);
    }
}
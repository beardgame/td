using Bearded.TD.Commands;
using Bearded.TD.Game.Players;

namespace Bearded.TD.Networking.Serialization
{
    interface IRequestSerializer<TObject>
    {
        IRequest<TObject> GetRequest(TObject game, Player sender);
        void Serialize(INetBufferStream stream);
    }
}
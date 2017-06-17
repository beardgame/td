using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Players;

namespace Bearded.TD.Networking.Serialization
{
    interface IRequestSerializer
    {
        IRequest GetRequest(GameInstance game, Player sender);
        void Serialize(INetBufferStream stream);
    }
}
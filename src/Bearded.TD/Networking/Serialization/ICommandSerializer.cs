using Bearded.TD.Commands;
using Bearded.TD.Game;

namespace Bearded.TD.Networking.Serialization
{
    interface ICommandSerializer
    {
        ICommand GetCommand(GameInstance game);
        void Serialize(INetBufferStream stream);
    }
}
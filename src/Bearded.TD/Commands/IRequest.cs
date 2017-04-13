using Bearded.TD.Game.Commands;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands
{
    interface IRequest
    {
        bool CheckPreconditions();

        ICommand ToCommand();

        IRequestSerializer Serializer { get; }
    }
}
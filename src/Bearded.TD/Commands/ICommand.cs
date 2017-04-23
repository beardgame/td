using Bearded.TD.Game.Commands;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands
{
    interface ICommand
    {
        void Execute();

        ICommandSerializer Serializer { get; }
    }

}
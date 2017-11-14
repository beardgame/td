using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands
{
    interface ICommand<in TContext>
    {
        void Execute();

        ICommandSerializer<TContext> Serializer { get; }
    }

}
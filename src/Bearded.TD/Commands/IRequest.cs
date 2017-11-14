using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands
{
    interface IRequest<in TContext, in TSender>
    {
        bool CheckPreconditions();

        ICommand<TContext> ToCommand();

        IRequestSerializer<TContext, TSender> Serializer { get; }
    }
}
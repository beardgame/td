using Bearded.TD.Commands.Serialization;

namespace Bearded.TD.Commands
{
    interface IRequest<TActor, TObject>
    {
        bool CheckPreconditions(TActor actor);

        ISerializableCommand<TObject> ToCommand();

        IRequestSerializer<TActor, TObject> Serializer { get; }
    }
}

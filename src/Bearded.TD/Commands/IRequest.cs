using Bearded.TD.Commands.Serialization;

namespace Bearded.TD.Commands
{
    interface IRequest<TObject>
    {
        bool CheckPreconditions();

        ISerializableCommand<TObject> ToCommand();

        IRequestSerializer<TObject> Serializer { get; }
    }
}
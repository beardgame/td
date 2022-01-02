using Bearded.TD.Commands.Serialization;

namespace Bearded.TD.Commands;

interface ISerializableCommand<TObject> : ICommand
{
    ICommandSerializer<TObject> Serializer { get; }
}
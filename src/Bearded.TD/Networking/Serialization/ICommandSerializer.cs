using Bearded.TD.Commands;

namespace Bearded.TD.Networking.Serialization
{
    interface ICommandSerializer<in TContext>
    {
        ICommand<TContext> GetCommand(TContext game);
        void Serialize(INetBufferStream stream);
    }
}
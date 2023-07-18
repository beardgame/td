namespace Bearded.TD.Commands;

sealed class ServerDispatcher<TObject> : BaseServerDispatcher<TObject>
{
    public ServerDispatcher(ICommandDispatcher<TObject> commandDispatcher)
        : base(commandDispatcher)
    {
    }
}
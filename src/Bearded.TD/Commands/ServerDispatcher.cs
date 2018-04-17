namespace Bearded.TD.Commands
{
    class ServerDispatcher<TObject> : BaseServerDispatcher<TObject>
    {
        public ServerDispatcher(ICommandDispatcher<TObject> commandDispatcher)
            : base(commandDispatcher)
        {
        }
    }
}
namespace Bearded.TD.Commands
{
    class ServerDispatcher<TContext> : BaseServerDispatcher<TContext>
    {
        public ServerDispatcher(ICommandDispatcher<TContext> commandDispatcher)
            : base(commandDispatcher)
        {
        }
    }
}
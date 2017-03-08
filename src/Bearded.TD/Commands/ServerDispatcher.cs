namespace Bearded.TD.Commands
{
    class ServerDispatcher : BaseServerDispatcher
    {
        public ServerDispatcher(ICommandDispatcher commandDispatcher)
            : base(commandDispatcher)
        {
        }
    }
}
namespace Bearded.TD.Commands
{
    class ServerDispatcher : BaseServerDispatcher
    {
        public static IDispatcher Default
            => new ServerDispatcher(
                new ServerCommandDispatcher(
                    new DefaultCommandExecutor()));

        public ServerDispatcher(ICommandDispatcher commandDispatcher)
            : base(commandDispatcher)
        {
        }
    }
}
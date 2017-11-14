namespace Bearded.TD.Commands
{
    interface ICommandExecutor<out TContext>
    {
        void Execute(ICommand<TContext> command);
    }

    class DefaultCommandExecutor<TContext> : ICommandExecutor<TContext>
    {
        public void Execute(ICommand<TContext> command)
        {
            command.Execute();
        }
    }
}
namespace Bearded.TD.Commands
{
    interface ICommandExecutor
    {
        void Execute(ICommand command);
    }

    class DefaultCommandExecutor : ICommandExecutor
    {
        public void Execute(ICommand command)
        {
            command.Execute();
        }
    }
}
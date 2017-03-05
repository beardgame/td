using System;

namespace Bearded.TD.Commands
{
    interface ICommandDispatcher
    {
        void Dispatch(ICommand command);
    }

    class ClientCommandDispatcher : ICommandDispatcher
    {
        private readonly ICommandExecutor executor;

        public ClientCommandDispatcher(ICommandExecutor executor)
        {
            this.executor = executor;
        }

        public void Dispatch(ICommand command)
        {
            executor.Execute(command);
        }
    }

    class ServerCommandDispatcher : ICommandDispatcher
    {
        private readonly ICommandExecutor executor;

        public ServerCommandDispatcher(ICommandExecutor executor)
        {
            this.executor = executor;
        }

        public void Dispatch(ICommand command)
        {
            // send to appropriate clients (usually all)

            executor.Execute(command);
        }
    }
}
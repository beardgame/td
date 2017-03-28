using System;

namespace Bearded.TD.Commands
{
    interface IRequestDispatcher
    {
        void Dispatch(IRequest request);
    }

    class ClientRequestDispatcher : IRequestDispatcher
    {
        public void Dispatch(IRequest request)
        {
            throw new NotImplementedException();
        }
    }

    class ServerRequestDispatcher : IRequestDispatcher
    {
        private readonly ICommandDispatcher commandDispatcher;

        public ServerRequestDispatcher(ICommandDispatcher commandDispatcher)
        {
            this.commandDispatcher = commandDispatcher;
        }

        public void Dispatch(IRequest request)
        {
            var command = request.CheckPreconditions()
                ? execute(request)
                : cancel(request);

            commandDispatcher.Dispatch(command);
        }

        private ICommand cancel(IRequest request)
        {
            return null;
        }

        private ICommand execute(IRequest request)
        {
            return request.ToCommand();
        }
    }
}
using Bearded.TD.Commands;

namespace Bearded.TD.Game.Commands
{
    abstract class UnifiedRequestCommand : IRequest, ICommand
    {
        public abstract bool CheckPreconditions();
        public ICommand ToCommand() => this;
        public abstract void Execute();
    }
}
using Bearded.TD.Commands;

namespace Bearded.TD.Game.Commands
{
    abstract class UnifiedRequestCommand : IRequest, ICommand
    {
        public abstract bool CheckPreconditions();
        public ICommand ToCommand() => this;
        public abstract void Execute();
    }

    abstract class UnifiedDebugRequestCommand : UnifiedRequestCommand
    {
        protected abstract bool CheckPreconditionsDebug();
#if DEBUG
        public override bool CheckPreconditions() => CheckPreconditionsDebug();
#else
        public override bool CheckPreconditions() => false;
#endif
    }
}
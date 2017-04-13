using Bearded.TD.Commands;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    interface IUnifiedRequestCommandSerializer : IRequestSerializer, ICommandSerializer
    {

    }

    abstract class UnifiedRequestCommand : IRequest, ICommand
    {
        public abstract bool CheckPreconditions();
        public ICommand ToCommand() => this;

        public abstract void Execute();

        protected abstract IUnifiedRequestCommandSerializer GetSerializer();

        IRequestSerializer IRequest.Serializer => GetSerializer();
        ICommandSerializer ICommand.Serializer => GetSerializer();
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
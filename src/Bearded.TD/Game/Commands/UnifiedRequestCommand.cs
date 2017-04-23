using Bearded.TD.Commands;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    abstract class UnifiedRequestCommandSerializer : IRequestSerializer, ICommandSerializer
    {
        public IRequest GetRequest(GameInstance game) => getSerialized(game);
        public ICommand GetCommand(GameInstance game) => getSerialized(game);

        protected abstract UnifiedRequestCommand getSerialized(GameInstance game);
        public abstract void Serialize(INetBufferStream stream);
    }

    abstract class UnifiedRequestCommand : IRequest, ICommand
    {
        public abstract bool CheckPreconditions();
        public ICommand ToCommand() => this;

        public abstract void Execute();

        protected abstract UnifiedRequestCommandSerializer GetSerializer();

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
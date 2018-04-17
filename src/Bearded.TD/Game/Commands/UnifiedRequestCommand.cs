using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    abstract class UnifiedRequestCommandSerializer : IRequestSerializer<GameInstance>, ICommandSerializer<GameInstance>
    {
        public IRequest<GameInstance> GetRequest(GameInstance game, Player player) => GetSerialized(game, player);
        public ISerializableCommand<GameInstance> GetCommand(GameInstance game) => GetSerialized(game, game.Me);

        protected abstract UnifiedRequestCommand GetSerialized(GameInstance game, Player player);
        public abstract void Serialize(INetBufferStream stream);
    }

    abstract class UnifiedRequestCommand : IRequest<GameInstance>, ISerializableCommand<GameInstance>
    {
        public abstract bool CheckPreconditions();
        public virtual ISerializableCommand<GameInstance> ToCommand() => this;

        public abstract void Execute();

        protected abstract UnifiedRequestCommandSerializer GetSerializer();

        IRequestSerializer<GameInstance> IRequest<GameInstance>.Serializer => GetSerializer();
        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => GetSerializer();
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
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    abstract class UnifiedRequestCommandSerializer : IRequestSerializer<GameInstance, Player>, ICommandSerializer<GameInstance>
    {
        public IRequest<GameInstance, Player> GetRequest(GameInstance game, Player player) => GetSerialized(game, player);
        public ICommand<GameInstance> GetCommand(GameInstance game) => GetSerialized(game, game.Me);

        protected abstract UnifiedRequestCommand GetSerialized(GameInstance game, Player player);
        public abstract void Serialize(INetBufferStream stream);
    }

    abstract class UnifiedRequestCommand : IRequest<GameInstance, Player>, ICommand<GameInstance>
    {
        public abstract bool CheckPreconditions();
        public virtual ICommand<GameInstance> ToCommand() => this;

        public abstract void Execute();

        protected abstract UnifiedRequestCommandSerializer GetSerializer();

        IRequestSerializer<GameInstance, Player> IRequest<GameInstance, Player>.Serializer => GetSerializer();
        ICommandSerializer<GameInstance> ICommand<GameInstance>.Serializer => GetSerializer();
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
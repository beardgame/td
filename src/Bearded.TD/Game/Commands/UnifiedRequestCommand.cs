using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    abstract class UnifiedRequestCommandSerializer : IRequestSerializer<Player, GameInstance>, ICommandSerializer<GameInstance>
    {
        public IRequest<Player, GameInstance> GetRequest(GameInstance game) => GetSerialized(game);
        public ISerializableCommand<GameInstance> GetCommand(GameInstance game) => GetSerialized(game);

        protected abstract UnifiedRequestCommand GetSerialized(GameInstance game);
        public abstract void Serialize(INetBufferStream stream);
    }

    abstract class UnifiedRequestCommand : IRequest<Player, GameInstance>, ISerializableCommand<GameInstance>
    {
        public abstract bool CheckPreconditions(Player actor);
        public virtual ISerializableCommand<GameInstance> ToCommand() => this;

        public abstract void Execute();

        protected abstract UnifiedRequestCommandSerializer GetSerializer();

        IRequestSerializer<Player, GameInstance> IRequest<Player, GameInstance>.Serializer => GetSerializer();
        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => GetSerializer();
    }

    abstract class UnifiedDebugRequestCommand : UnifiedRequestCommand
    {
        protected virtual bool CheckPreconditionsDebug(Player actor) => true;
#if DEBUG
        public sealed override bool CheckPreconditions(Player actor) => CheckPreconditionsDebug(actor);
#else
        public sealed override bool CheckPreconditions(Player _) => false;
#endif
    }

    abstract class UnifiedDebugRequestCommandWithoutParameter<TSelf> :
        IRequest<Player, GameInstance>,
        ISerializableCommand<GameInstance>,
        IRequestSerializer<Player, GameInstance>,
        ICommandSerializer<GameInstance>
        where TSelf : UnifiedDebugRequestCommandWithoutParameter<TSelf>, new()
    {
        protected GameInstance Game { get; private set; }

        public static UnifiedDebugRequestCommandWithoutParameter<TSelf> For(GameInstance game)
            => new TSelf { Game = game };

        public abstract void Execute();

        protected virtual bool CheckPreconditionsDebug(Player actor) => true;

#if DEBUG
        public bool CheckPreconditions(Player actor) => CheckPreconditionsDebug(actor);
#else
        public bool CheckPreconditions(Player _) => false;
#endif

        IRequest<Player, GameInstance> IRequestSerializer<Player, GameInstance>.GetRequest(GameInstance game) => For(game);
        ISerializableCommand<GameInstance> ICommandSerializer<GameInstance>.GetCommand(GameInstance game) => For(game);

        ISerializableCommand<GameInstance> IRequest<Player, GameInstance>.ToCommand() => this;
        IRequestSerializer<Player, GameInstance> IRequest<Player, GameInstance>.Serializer => this;
        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => this;

        public void Serialize(INetBufferStream stream) { }
    }
}

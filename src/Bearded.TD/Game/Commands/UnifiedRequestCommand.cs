using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    abstract class UnifiedRequestCommandSerializer : IRequestSerializer<GameInstance>, ICommandSerializer<GameInstance>
    {
        public IRequest<GameInstance> GetRequest(GameInstance game) => GetSerialized(game);
        public ISerializableCommand<GameInstance> GetCommand(GameInstance game) => GetSerialized(game);

        protected abstract UnifiedRequestCommand GetSerialized(GameInstance game);
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
        protected virtual bool CheckPreconditionsDebug() => true;
#if DEBUG
        public sealed override bool CheckPreconditions() => CheckPreconditionsDebug();
#else
        public sealed override bool CheckPreconditions() => false;
#endif
    }

    abstract class UnifiedDebugRequestCommandWithoutParameter<TSelf> :
        IRequest<GameInstance>,
        ISerializableCommand<GameInstance>,
        IRequestSerializer<GameInstance>,
        ICommandSerializer<GameInstance>
        where TSelf : UnifiedDebugRequestCommandWithoutParameter<TSelf>, new()
    {
        protected GameInstance Game { get; private set; }
        
        public static UnifiedDebugRequestCommandWithoutParameter<TSelf> For(GameInstance game)
            => new TSelf { Game = game };
        
        public abstract void Execute();
        
        protected virtual bool CheckPreconditionsDebug() => true;
        
#if DEBUG
        public bool CheckPreconditions() => CheckPreconditionsDebug();
#else
        public bool CheckPreconditions() => false;
#endif

        IRequest<GameInstance> IRequestSerializer<GameInstance>.GetRequest(GameInstance game) => For(game);
        ISerializableCommand<GameInstance> ICommandSerializer<GameInstance>.GetCommand(GameInstance game) => For(game);
        
        ISerializableCommand<GameInstance> IRequest<GameInstance>.ToCommand() => this;
        IRequestSerializer<GameInstance> IRequest<GameInstance>.Serializer => this;
        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => this;
        
        public void Serialize(INetBufferStream stream) { }
    }
}

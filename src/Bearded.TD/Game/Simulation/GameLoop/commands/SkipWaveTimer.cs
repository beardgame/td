using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.GameLoop;

static class SkipWaveTimer
{
    public static IRequest<Player, GameInstance> Request(GameInstance game)
        => new Implementation(game);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private readonly GameInstance game;

        public Implementation(GameInstance game)
        {
            this.game = game;
        }

        public override bool CheckPreconditions(Player actor)
        {
            return true;
        }

        public override ISerializableCommand<GameInstance> ToCommand() => this;

        public override void Execute()
        {
            game.Meta.Events.Send(new Game.GameLoop.SkipWaveTimer());
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer();
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        [UsedImplicitly]
        public Serializer()
        {
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
        {
            return new Implementation(game);
        }

        public override void Serialize(INetBufferStream stream)
        {
        }
    }
}

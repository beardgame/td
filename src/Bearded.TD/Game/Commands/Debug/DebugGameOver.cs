using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands.Debug
{
    static class DebugGameOver
    {
        public static IRequest<Player, GameInstance> Request(GameState game)
            => new Implementation(game);

        private sealed class Implementation : UnifiedDebugRequestCommand
        {
            private readonly GameState game;

            public Implementation(GameState game)
            {
                this.game = game;
            }

            public override void Execute() => game.Meta.DoGameOver();

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer();
        }

        private sealed class Serializer : UnifiedRequestCommandSerializer
        {
            // ReSharper disable once EmptyConstructor
            public Serializer() { }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
                => new Implementation(game.State);

            public override void Serialize(INetBufferStream stream) { }
        }
    }
}

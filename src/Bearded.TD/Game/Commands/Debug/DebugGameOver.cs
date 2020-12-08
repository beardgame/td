using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands.Debug
{
    static class DebugGameOver
    {
        public static IRequest<Player, GameInstance> Request(GameState.GameState game)
            => new Implementation(game);

        private class Implementation : UnifiedDebugRequestCommand
        {
            private readonly GameState.GameState game;

            public Implementation(GameState.GameState game)
            {
                this.game = game;
            }

            public override void Execute() => game.Meta.DoGameOver();

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer();
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            // ReSharper disable once EmptyConstructor
            public Serializer() { }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
                => new Implementation(game.State);

            public override void Serialize(INetBufferStream stream) { }
        }
    }
}

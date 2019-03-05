using Bearded.TD.Commands;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands.Debug
{
    static class DebugGameOver
    {
        public static IRequest<GameInstance> Request(GameState game)
            => new Implementation(game);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly GameState game;

            public Implementation(GameState game)
            {
                this.game = game;
            }

            public override bool CheckPreconditions() => DebugGuards.IsInDebugMode;

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

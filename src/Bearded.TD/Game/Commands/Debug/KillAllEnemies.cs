using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Units;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands.Debug
{
    static class KillAllEnemies
    {
        public static IRequest<GameInstance> Request(GameInstance game)
            => new Implementation(game);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly GameInstance game;

            public Implementation(GameInstance game)
            {
                this.game = game;
            }

            public override bool CheckPreconditions() => DebugGuards.IsInDebugMode;

            public override void Execute()
            {
                foreach (var enemy in game.State.GameObjects.OfType<EnemyUnit>())
                {
                    enemy.Kill(game.State.RootFaction);
                }
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer();
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game) => new Implementation(game);

            public override void Serialize(INetBufferStream stream) { }
        }
    }
}

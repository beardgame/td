using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Networking.Serialization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Debug;

static class KillAllEnemies
{
    public static IRequest<Player, GameInstance> Request(GameInstance game, IDamageSource? damageSource)
        => new Implementation(game, damageSource);

    private sealed class Implementation : UnifiedDebugRequestCommand
    {
        private readonly GameInstance game;
        private readonly IDamageSource? damageSource;

        public Implementation(GameInstance game, IDamageSource? damageSource)
        {
            this.game = game;
            this.damageSource = damageSource;
        }

        public override void Execute()
        {
            foreach (var enemy in game.State.GameObjects.OfType<EnemyUnit>())
            {
                enemy.Kill(damageSource);
            }
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(damageSource);

        private sealed class Serializer : UnifiedRequestCommandSerializer
        {
            private readonly DamageSourceSerializer damageSourceSerializer = new();

            [UsedImplicitly]
            public Serializer() { }

            public Serializer(IDamageSource? damageSource)
            {
                damageSourceSerializer.Populate(damageSource);
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
                => new Implementation(game, damageSourceSerializer.ToDamageSource(game));

            public override void Serialize(INetBufferStream stream)
            {
                damageSourceSerializer.Serialize(stream);
            }
        }
    }
}
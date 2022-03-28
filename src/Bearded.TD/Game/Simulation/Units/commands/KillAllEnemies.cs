using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities.Linq;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Units;

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
            var enemyLives = game.State.GameObjects
                .OfType<GameObject>()
                .Select(obj => obj.GetComponents<IEnemyLife>().SingleOrDefault())
                .NotNull();
            foreach (var enemy in enemyLives)
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

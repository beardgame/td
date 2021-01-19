using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class KillUnit
    {
        public static ISerializableCommand<GameInstance> Command(EnemyUnit unit, IDamageSource? damageSource) =>
            new Implementation(unit, damageSource);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly EnemyUnit unit;
            private readonly IDamageSource? damageSource;

            public Implementation(EnemyUnit unit, IDamageSource? damageSource)
            {
                this.unit = unit;
                this.damageSource = damageSource;
            }

            public void Execute() => unit.Kill(damageSource);
            public ICommandSerializer<GameInstance> Serializer => new Serializer(unit, damageSource);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<EnemyUnit> unit;
            private readonly DamageSourceSerializer damageSourceSerializer = new();

            [UsedImplicitly] public Serializer() { }

            public Serializer(EnemyUnit unit, IDamageSource? damageSource)
            {
                this.unit = unit.Id;
                damageSourceSerializer.Populate(damageSource);
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            {
                return new Implementation(
                    game.State.Find(unit),
                    damageSourceSerializer.ToDamageSource(game));
            }

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref unit);
                damageSourceSerializer.Serialize(stream);
            }
        }
    }
}

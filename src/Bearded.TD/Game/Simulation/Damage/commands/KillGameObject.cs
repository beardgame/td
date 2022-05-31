using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Damage;

static class KillGameObject
{
    public static ISerializableCommand<GameInstance> Command(GameObject obj, IDamageSource? damageSource) =>
        new Implementation(obj, damageSource);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameObject obj;
        private readonly IDamageSource? damageSource;

        public Implementation(GameObject obj, IDamageSource? damageSource)
        {
            this.obj = obj;
            this.damageSource = damageSource;
        }

        public void Execute() => obj.GetComponents<IKillable>().Single().Kill(damageSource);
        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(obj, damageSource);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<GameObject> obj;
        private readonly DamageSourceSerializer damageSourceSerializer = new();

        [UsedImplicitly] public Serializer() { }

        public Serializer(GameObject obj, IDamageSource? damageSource)
        {
            this.obj = obj.FindId();
            damageSourceSerializer.Populate(damageSource);
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
        {
            return new Implementation(
                game.State.Find(obj),
                damageSourceSerializer.ToDamageSource(game));
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref obj);
            damageSourceSerializer.Serialize(stream);
        }
    }
}

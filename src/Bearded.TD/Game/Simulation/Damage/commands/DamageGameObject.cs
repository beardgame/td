using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Damage;

static class DamageGameObject
{
    public static ISerializableCommand<GameInstance> Command(
        GameObject source, GameObject target, TypedDamage damage, Hit hit) =>
        new Implementation(source, target, damage, hit);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameObject source;
        private readonly GameObject target;
        private readonly TypedDamage damage;
        private readonly Hit hit;

        public Implementation(GameObject source, GameObject target, TypedDamage damage, Hit hit)
        {
            this.source = source;
            this.target = target;
            this.damage = damage;
            this.hit = hit;
        }

        public void Execute()
        {
            var executor = DamageExecutor.FromObject(source);
            executor.TryDoDamage(target, damage, hit);
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(source, target, damage, hit);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<GameObject> source;
        private Id<GameObject> target;
        private TypedDamage damage;
        private Hit hit;

        [UsedImplicitly]
        public Serializer() { }

        public Serializer(GameObject source, GameObject target, TypedDamage damage, Hit hit)
        {
            this.source = source.FindId();
            this.target = target.FindId();
            this.damage = damage;
            this.hit = hit;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            => new Implementation(game.State.Find(source), game.State.Find(target), damage, hit);

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref source);
            stream.Serialize(ref target);
            stream.Serialize(ref damage);
            stream.Serialize(ref hit);
        }
    }
}

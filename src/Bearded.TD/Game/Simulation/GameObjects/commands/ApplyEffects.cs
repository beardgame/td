using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface IEffectApplier
{
    void ApplyTo(GameObject target);
}

static class ApplyEffects
{
    public static ISerializableCommand<GameInstance> Command(GameObject source, GameObject target) =>
        new Implementation(source, target);

    private sealed class Implementation(GameObject source, GameObject target)
        : ISerializableCommand<GameInstance>
    {
        public void Execute()
        {
            if (source.TryGetSingleComponent<IEffectApplier>(out var applier))
            {
                applier.ApplyTo(target);
            }
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer
            => new Serializer(source, target);
    }

    private sealed class Serializer() : ICommandSerializer<GameInstance>
    {
        private Id<GameObject> source;
        private Id<GameObject> target;

        public Serializer(GameObject source, GameObject target) : this()
        {
            this.source = source.FindId();
            this.target = target.FindId();
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
        {
            return new Implementation(
                game.State.Find(source),
                game.State.Find(target)
            );
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref source);
            stream.Serialize(ref target);
        }
    }
}

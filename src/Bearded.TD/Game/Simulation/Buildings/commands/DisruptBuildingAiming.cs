using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Elements.Phenomena;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Buildings;

static class DisruptBuildingAiming
{
    public static ISerializableCommand<GameInstance> Command(GameObject obj, TimeSpan duration) =>
        new Implementation(obj, duration);

    private sealed class Implementation(GameObject obj, TimeSpan duration)
        : ISerializableCommand<GameInstance>
    {
        public void Execute()
        {
            obj.TryApplyEffect(new DisruptedAiming.Effect(duration));
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer
            => new Serializer(obj, duration);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<GameObject> obj;
        private double duration;

        [UsedImplicitly] public Serializer() { }

        public Serializer(GameObject obj, TimeSpan duration)
        {
            this.obj = obj.FindId();
            this.duration = duration.NumericValue;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
        {
            return new Implementation(game.State.Find(obj), duration.S());
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref obj);
            stream.Serialize(ref duration);
        }
    }
}

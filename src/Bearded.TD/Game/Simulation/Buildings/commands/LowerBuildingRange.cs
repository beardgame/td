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

static class LowerBuildingRange
{
    public static ISerializableCommand<GameInstance> Command(GameObject obj, TimeSpan duration, double factor) =>
        new Implementation(obj, duration, factor);

    private sealed class Implementation(GameObject obj, TimeSpan duration, double factor)
        : ISerializableCommand<GameInstance>
    {
        public void Execute()
        {
            if (obj.TryGetSingleComponent<IElementSystemEntity>(out var entity))
            {
                entity.ApplyEffect(new LowerWeaponRange.Effect(factor, duration));
            }
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer
            => new Serializer(obj, duration, factor);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<GameObject> obj;
        private double duration;
        private double factor;

        [UsedImplicitly] public Serializer() { }

        public Serializer(GameObject obj, TimeSpan duration, double factor)
        {
            this.obj = obj.FindId();
            this.duration = duration.NumericValue;
            this.factor = factor;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
        {
            return new Implementation(game.State.Find(obj), duration.S(), factor);
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref obj);
            stream.Serialize(ref duration);
            stream.Serialize(ref factor);
        }
    }
}

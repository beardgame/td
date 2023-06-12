using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using JetBrains.Annotations;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons;

static class JamWeapon
{
    public static ISerializableCommand<GameInstance> Command(GameObject weapon, TimeSpan duration) =>
        new Implementation(weapon, duration);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameObject weapon;
        private readonly TimeSpan duration;

        public Implementation(GameObject weapon, TimeSpan duration)
        {
            this.weapon = weapon;
            this.duration = duration;
        }

        public void Execute()
        {
            if (weapon.TryGetSingleComponent<IWeaponJammer>(out var jammer))
            {
                jammer.Jam(duration);
            }
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(weapon, duration);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<GameObject> weapon;
        private double duration;

        [UsedImplicitly]
        public Serializer() { }

        public Serializer(GameObject weapon, TimeSpan duration)
        {
            this.weapon = weapon.FindId();
            this.duration = duration.NumericValue;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            => new Implementation(game.State.Find(weapon), duration.S());

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref weapon);
            stream.Serialize(ref duration);
        }
    }
}

using System.Linq;
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
        private Id<GameObject> building;
        private int weaponId;
        private double duration;

        [UsedImplicitly]
        public Serializer() { }

        public Serializer(GameObject weapon, TimeSpan duration)
        {
            var buildingObj = weapon.Parent!;
            building = buildingObj.FindId();
            weaponId = buildingObj.GetComponents<ITurret>().TakeWhile(turret => turret.Weapon != weapon).Count();
            this.duration = duration.NumericValue;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
        {
            var buildingObj = game.State.Find(building);
            var weapon = buildingObj.GetComponents<ITurret>().ElementAt(weaponId).Weapon;
            return new Implementation(weapon, duration.S());
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref building);
            stream.Serialize(ref weaponId);
            stream.Serialize(ref duration);
        }
    }
}

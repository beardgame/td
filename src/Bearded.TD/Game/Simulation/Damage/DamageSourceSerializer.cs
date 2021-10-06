using System;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Damage
{
    sealed class DamageSourceSerializer
    {
        private enum SupportedImplementation : byte
        {
            None,
            Building,
            Enemy,
            DivineIntervention,
        }

        private byte type;
        private int id;

        public void Populate(IDamageSource? damageSource)
        {
            switch (damageSource)
            {
                case null:
                    type = (byte) SupportedImplementation.None;
                    break;
                case DamageSource<Building> b:
                    type = (byte) SupportedImplementation.Building;
                    id = b.Id.Value;
                    break;
                case DamageSource<EnemyUnit> e:
                    type = (byte) SupportedImplementation.Enemy;
                    id = e.Id.Value;
                    break;
                case DivineIntervention:
                    type = (byte) SupportedImplementation.DivineIntervention;
                    break;
            }
        }

        public IDamageSource? ToDamageSource(GameInstance instance)
        {
            return type switch
            {
                (byte) SupportedImplementation.None => null,
                (byte) SupportedImplementation.Building => instance.State.Find(new Id<Building>(id)).TryGetSingleComponent<IDamageSource>(out var s) ? s : null,
                (byte) SupportedImplementation.Enemy => instance.State.Find(new Id<EnemyUnit>(id)).TryGetSingleComponent<IDamageSource>(out var s) ? s : null,
                (byte) SupportedImplementation.DivineIntervention => DivineIntervention.DamageSource,
                _ => throw new IndexOutOfRangeException()
            };
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref type);
            stream.Serialize(ref id);
        }
    }
}

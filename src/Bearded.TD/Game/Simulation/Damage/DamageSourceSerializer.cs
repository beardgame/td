using System;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Damage
{
    sealed class DamageSourceSerializer
    {
        private enum SupportedImplementation
        {
            None,
            Building,
            Enemy,
            DivineIntervention,
        }

        private SupportedImplementation type;
        private int id;

        public void Populate(IDamageSource? damageSource)
        {
            switch (damageSource)
            {
                case null:
                    type = SupportedImplementation.None;
                    break;
                case Building building:
                    type = SupportedImplementation.Building;
                    id = building.Id.Value;
                    break;
                case EnemyUnit enemy:
                    type = SupportedImplementation.Enemy;
                    id = enemy.Id.Value;
                    break;
                case DivineIntervention:
                    type = SupportedImplementation.DivineIntervention;
                    break;
            }
        }

        public IDamageSource? ToDamageSource(GameInstance instance)
        {
            return type switch
            {
                SupportedImplementation.None => null,
                SupportedImplementation.Building => instance.State.Find(new Id<Building>(id)),
                SupportedImplementation.Enemy => instance.State.Find(new Id<EnemyUnit>(id)),
                SupportedImplementation.DivineIntervention => DivineIntervention.DamageSource,
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

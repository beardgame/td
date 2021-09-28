using System;

namespace Bearded.TD.Game.Simulation.Damage
{
    sealed class DivineIntervention : IDamageSource
    {
        public static DivineIntervention DamageSource { get; } = new();

        private DivineIntervention() {}

        public int OwnerIdValue => throw new NotSupportedException();
        public void AttributeDamage(IDamageTarget target, DamageResult result) {}
        public void AttributeKill(IDamageTarget target) {}
    }
}

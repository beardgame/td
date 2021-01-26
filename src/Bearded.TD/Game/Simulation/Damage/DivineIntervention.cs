namespace Bearded.TD.Game.Simulation.Damage
{
    sealed class DivineIntervention : IDamageSource
    {
        public static DivineIntervention DamageSource { get; } = new();

        private DivineIntervention() {}

        public void AttributeDamage(IMortal target, DamageResult result) {}
        public void AttributeKill(IMortal target) {}
    }
}

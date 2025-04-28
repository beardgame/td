namespace Bearded.TD.Game.Simulation.Damage;

sealed class HealthDamagePreview(TypedDamage unmodifiedDamage)
    : DamagePreview(unmodifiedDamage.Type, unmodifiedDamage.Untyped());

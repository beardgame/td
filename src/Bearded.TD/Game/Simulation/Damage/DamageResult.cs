namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct DamageResult(TypedDamage TypedDamage, HitPoints DiscreteDifference);

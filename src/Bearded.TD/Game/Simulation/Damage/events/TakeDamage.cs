using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct TakeDamage(TypedDamage Damage, IDamageSource? Source) : IComponentEvent;

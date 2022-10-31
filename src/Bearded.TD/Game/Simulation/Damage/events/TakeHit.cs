using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct TakeHit(HitContext Context, TypedDamage IntendedDamage, TypedDamage ActualDamage) : IComponentEvent;

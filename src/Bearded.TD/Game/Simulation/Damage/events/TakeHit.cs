using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct TakeHit(Hit Context, TypedDamage ActualDamage) : IComponentEvent;

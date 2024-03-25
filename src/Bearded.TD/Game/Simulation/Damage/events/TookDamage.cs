using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct TookDamage(IDamageSource? Source) : IComponentEvent;

using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct TakeDamage(DamageResult Damage, IDamageSource? Source) : IComponentEvent;
using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly record struct TakeDamage(DamageResult Damage, IDamageSource? Source) : IComponentEvent;
}

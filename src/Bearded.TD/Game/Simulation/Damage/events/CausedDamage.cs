using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly record struct CausedDamage(DamageResult Result) : IComponentEvent;
}

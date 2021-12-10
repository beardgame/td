using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly record struct HealDamage(HealResult Heal) : IComponentEvent;
}

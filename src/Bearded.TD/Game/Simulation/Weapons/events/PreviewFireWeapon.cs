using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Weapons;

readonly record struct PreviewFireWeapon(UntypedDamage Damage, bool IsCancelled) : IComponentPreviewEvent
{
    public PreviewFireWeapon Cancelled() => this with { IsCancelled = true };
}

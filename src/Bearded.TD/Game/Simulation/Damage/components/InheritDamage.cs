using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

[Component("inheritDamage")]
sealed class InheritDamage : Component, IProperty<UntypedDamage>
{
    public UntypedDamage Value { get; private set; }

    protected override void OnAdded()
    {
        if (Owner.Parent is { } p && p.TryGetProperty<UntypedDamage>(out var dmg))
        {
            Value = dmg;
        }
    }

    public override void Update(TimeSpan elapsedTime) { }
}

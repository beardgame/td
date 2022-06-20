using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

sealed class DynamicDamage : Component, IProperty<UntypedDamage>
{
    public UntypedDamage Value { get; private set; }

    public DynamicDamage()
    {
        Value = UntypedDamage.Zero;
    }

    public void Inject(UntypedDamage value)
    {
        Value = value;
    }

    protected override void OnAdded() {}

    public override void Update(TimeSpan elapsedTime) {}
}

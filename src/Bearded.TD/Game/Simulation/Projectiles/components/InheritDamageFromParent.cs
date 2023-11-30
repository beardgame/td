using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("inheritDamageFromParent")]
sealed class InheritDamageFromParent : Component
{
    public override void Activate()
    {
        base.Activate();

        if (Owner.Parent?.TryGetProperty<UntypedDamage>(out var damage) ?? false)
        {
            Owner.AddComponent(new Property<UntypedDamage>(damage));
        }

        Owner.RemoveComponent(this);
    }

    protected override void OnAdded() { }
    public override void Update(TimeSpan elapsedTime) { }
}

using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("inheritSource")]
sealed class InheritSource : Component
{
    public override void Activate()
    {
        base.Activate();

        if (Owner.TryGetSingleComponentInOwnerTree<IProperty<Source>>(out var property))
        {
            Owner.AddComponent(new Property<Source>(property.Value));
        }
    }

    protected override void OnAdded() { }
    public override void Update(TimeSpan elapsedTime) { }
}

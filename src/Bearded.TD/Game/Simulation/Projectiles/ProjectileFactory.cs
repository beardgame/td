using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

static class ProjectileFactory
{
    public static GameObject Create(
        IComponentOwnerBlueprint blueprint,
        GameObject parent,
        Position3 position,
        Direction2 direction,
        Velocity3 muzzleVelocity,
        UntypedDamage damage)
    {
        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(blueprint, parent, position, direction);

        obj.AddComponent(new ParabolicMovement(muzzleVelocity));
        obj.AddComponent(new PointCollider());
        obj.AddComponent(new Property<UntypedDamage>(damage));

        return obj;
    }
}

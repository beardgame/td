using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

static class ProjectileFactory
{
    public static GameObject CreateTemplate(IComponentOwnerBlueprint blueprint, IComponentOwner)

    public static GameObject Create(
        IComponentOwnerBlueprint blueprint,
        IComponentOwner parent,
        GameObjectUpgradeSidecar upgradeSidecar,
        Position3 position,
        Direction2 direction,
        Velocity3 muzzleVelocity,
        UntypedDamage damage)
    {
        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(blueprint, parent, position, direction);

        obj.AddComponent(new ParabolicMovement(muzzleVelocity));
        obj.AddComponent(new Property<UntypedDamage>(damage));

        upgradeSidecar.ApplyUpgrades(obj);

        return obj;
    }
}

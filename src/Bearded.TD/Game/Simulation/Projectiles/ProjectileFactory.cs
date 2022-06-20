using System.Linq;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

static class ProjectileFactory
{
    public static GameObject Create(
        GameState game,
        IComponentOwnerBlueprint blueprint,
        IComponentOwner parent,
        Position3 position,
        Direction2 direction,
        Velocity3 muzzleVelocity,
        UntypedDamage damage)
    {
        var obj = ComponentGameObjectFactory.CreateFromBlueprintWithDefaultRenderer(
            game, blueprint, parent, position, direction);

        obj.AddComponent(new ParabolicMovement(muzzleVelocity));
        obj.AddComponent(new Property<UntypedDamage>(damage));

        applyCurrentUpgradesTo(parent, obj);

        return obj;
    }

    private static void applyCurrentUpgradesTo(IComponentOwner parent, GameObject projectile)
    {
        if (!parent.TryGetSingleComponentInOwnerTree<IBuildingUpgradeManager>(out var upgradeManager))
        {
            return;
        }

        var upgrades = upgradeManager.AppliedUpgrades.Where(u => u.CanApplyTo(projectile));
        foreach (var upgrade in upgrades)
        {
            upgrade.ApplyTo(projectile);
        }
    }
}

using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

sealed class UpgradableProjectileFactory
{
    private readonly IComponentOwnerBlueprint blueprint;
    private readonly GameObject parent;
    private readonly GameObjectUpgradeCopier upgradeCopier;

    public UpgradableProjectileFactory(IComponentOwnerBlueprint blueprint, GameObject parent)
    {
        this.blueprint = blueprint;
        this.parent = parent;

        var projectileTemplate = ProjectileFactory.Create(
            this.blueprint, this.parent, Position3.Zero, Direction2.Zero, Velocity3.Zero, UntypedDamage.Zero);
        upgradeCopier = GameObjectUpgradeCopier.ForTemplateObject(projectileTemplate);
    }

    public void PreviewUpgrade(IUpgradePreview upgradePreview)
    {
        upgradeCopier.PreviewUpgrade(upgradePreview);
    }

    public GameObject Create(
        Position3 position,
        Direction2 direction,
        Velocity3 muzzleVelocity,
        UntypedDamage damage)
    {
        var projectile = ProjectileFactory.Create(blueprint, parent, position, direction, muzzleVelocity, damage);
        upgradeCopier.CopyUpgradesTo(projectile);
        return projectile;
    }
}

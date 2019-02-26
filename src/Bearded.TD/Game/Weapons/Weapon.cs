using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Rendering;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Weapons
{
    [ComponentOwner]
    class Weapon : IPositionable, IFactioned
    {
        private readonly ITurret turret;
        private readonly Building ownerAsBuilding;

        private readonly ComponentCollection<Weapon> components = new ComponentCollection<Weapon>();

        public Direction2 AimDirection { get; private set; }
        public bool ShootingThisFrame { get; private set; }

        public GameObject Owner => turret.Owner;
        public Position2 Position => turret.Position;
        public Faction Faction => turret.OwnerFaction;

        public Weapon(IWeaponBlueprint blueprint, ITurret turret)
        {
            this.turret = turret;
            ownerAsBuilding = turret.Owner as Building;

            components.Add(this, blueprint.GetComponents());
        }

        public bool CanApplyUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.CanApplyTo(components);

        public void ApplyUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.ApplyTo(components);

        public bool RemoveUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.RemoveFrom(components);

        public void ShootIn(Direction2 direction)
        {
            AimDirection = direction;
            ShootingThisFrame = true;
        }

        public void Update(TimeSpan elapsedTime)
        {
            if (ownerAsBuilding == null || !ownerAsBuilding.IsCompleted)
                return;

            // TODO: component order currently matters - if the order is inverter, the weapon will not shoot
            // this needs a proper solution (event/subscription based system?) to handle communication between components
            // and it has to work no matter what order components are ordered in
            ShootingThisFrame = false;

            components.Update(elapsedTime);
        }

        public void Draw(GeometryManager geometries)
        {
            components.Draw(geometries);
        }
    }
}

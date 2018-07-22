using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Weapons;
using Bearded.TD.Game.World;

namespace Bearded.TD.Game
{
    sealed class Blueprints
    {
        public ReadonlyBlueprintCollection<FootprintGroup> Footprints { get; }
        public ReadonlyBlueprintCollection<IBuildingBlueprint> Buildings { get; }
        public ReadonlyBlueprintCollection<IUnitBlueprint> Units { get; }
        public ReadonlyBlueprintCollection<IWeaponBlueprint> Weapons { get; }
        public ReadonlyBlueprintCollection<IProjectileBlueprint> Projectiles { get; }

        public Blueprints(ReadonlyBlueprintCollection<FootprintGroup> footprints,
                ReadonlyBlueprintCollection<IBuildingBlueprint> buildings,
                ReadonlyBlueprintCollection<IUnitBlueprint> units,
                ReadonlyBlueprintCollection<IWeaponBlueprint> weapons,
                ReadonlyBlueprintCollection<IProjectileBlueprint> projectiles)
        {
            Footprints = footprints;
            Buildings = buildings;
            Units = units;
            Weapons = weapons;
            Projectiles = projectiles;
        }

        public static Blueprints Merge(IEnumerable<Blueprints> blueprints)
        {
            var blueprintsList = blueprints as IList<Blueprints> ?? blueprints.ToList();
            return new Blueprints(
                new ReadonlyBlueprintCollection<FootprintGroup>(
                        blueprintsList.SelectMany(blueprint => blueprint.Footprints.All)),
                new ReadonlyBlueprintCollection<IBuildingBlueprint>(
                        blueprintsList.SelectMany(blueprint => blueprint.Buildings.All)),
                new ReadonlyBlueprintCollection<IUnitBlueprint>(
                        blueprintsList.SelectMany(blueprint => blueprint.Units.All)),
                new ReadonlyBlueprintCollection<IWeaponBlueprint>(
                        blueprintsList.SelectMany(blueprint => blueprint.Weapons.All)),
                new ReadonlyBlueprintCollection<IProjectileBlueprint>(
                        blueprintsList.SelectMany(blueprint => blueprint.Projectiles.All)));
        }
    }
}

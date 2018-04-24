using Bearded.TD.Mods.Models;

namespace Bearded.TD.Mods
{
    sealed class Mod
    {
        public string Name { get; }
        public string Id { get; }

        public Blueprints Blueprints { get; }
        
        public Mod(
            ReadonlyBlueprintCollection<FootprintGroup> footprints,
            ReadonlyBlueprintCollection<BuildingBlueprint> buildings,
            ReadonlyBlueprintCollection<UnitBlueprint> units,
            ReadonlyBlueprintCollection<WeaponBlueprint> weapons,
            ReadonlyBlueprintCollection<ProjectileBlueprint> projectiles)
        {
            Blueprints = new Blueprints(footprints, buildings, units, weapons, projectiles);
        }
    }
}

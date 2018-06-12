using System.Collections.Generic;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Mods
{
    sealed class Mod
    {
        public string Id { get; }
        public string Name { get; }

        public Blueprints Blueprints { get; }
        public IDictionary<string, UpgradeTag> Tags { get; }
        
        public Mod(
            string id,
            string name,
            ReadonlyBlueprintCollection<FootprintGroup> footprints,
            ReadonlyBlueprintCollection<BuildingBlueprint> buildings,
            ReadonlyBlueprintCollection<UnitBlueprint> units,
            ReadonlyBlueprintCollection<WeaponBlueprint> weapons,
            ReadonlyBlueprintCollection<ProjectileBlueprint> projectiles,
            IDictionary<string, UpgradeTag> tags)
        {
            Id = id;
            Name = name;
            Blueprints = new Blueprints(footprints, buildings, units, weapons, projectiles);
            Tags = tags;
        }
    }
}

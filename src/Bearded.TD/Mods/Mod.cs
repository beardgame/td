using System.Collections.Generic;
using Bearded.TD.Game;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Technologies;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Weapons;
using Bearded.TD.Game.World;

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
            ReadonlyBlueprintCollection<IBuildingBlueprint> buildings,
            ReadonlyBlueprintCollection<IUnitBlueprint> units,
            ReadonlyBlueprintCollection<IWeaponBlueprint> weapons,
            ReadonlyBlueprintCollection<IProjectileBlueprint> projectiles,
            IDictionary<string, UpgradeTag> tags)
        {
            Id = id;
            Name = name;
            Blueprints = new Blueprints(footprints, buildings, units, weapons, projectiles);
            Tags = tags;
        }
    }
}

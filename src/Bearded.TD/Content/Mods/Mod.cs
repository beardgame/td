using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Game.Weapons;
using Bearded.TD.Game.World;
using Bearded.Utilities;

namespace Bearded.TD.Content.Mods
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
            ReadonlyBlueprintCollection<SpriteSet> sprites,
            ReadonlyBlueprintCollection<FootprintGroup> footprints,
            ReadonlyBlueprintCollection<IBuildingBlueprint> buildings,
            ReadonlyBlueprintCollection<IUnitBlueprint> units,
            ReadonlyBlueprintCollection<IWeaponBlueprint> weapons,
            ReadonlyBlueprintCollection<IProjectileBlueprint> projectiles,
            ImmutableDictionary<Id<UpgradeBlueprint>, UpgradeBlueprint> upgrades,
            IDictionary<string, UpgradeTag> tags)
        {
            Id = id;
            Name = name;
            Blueprints = new Blueprints(sprites, footprints, buildings, units, weapons, projectiles, upgrades);
            Tags = tags;
        }
    }
}

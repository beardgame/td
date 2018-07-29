using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Weapons;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Game
{
    sealed class Blueprints
    {
        public ReadonlyBlueprintCollection<SpriteSet> Sprites { get; }
        public ReadonlyBlueprintCollection<FootprintGroup> Footprints { get; }
        public ReadonlyBlueprintCollection<IBuildingBlueprint> Buildings { get; }
        public ReadonlyBlueprintCollection<IUnitBlueprint> Units { get; }
        public ReadonlyBlueprintCollection<IWeaponBlueprint> Weapons { get; }
        public ReadonlyBlueprintCollection<IProjectileBlueprint> Projectiles { get; }

        public Blueprints(ReadonlyBlueprintCollection<SpriteSet> sprites,
            ReadonlyBlueprintCollection<FootprintGroup> footprints,
            ReadonlyBlueprintCollection<IBuildingBlueprint> buildings,
            ReadonlyBlueprintCollection<IUnitBlueprint> units,
            ReadonlyBlueprintCollection<IWeaponBlueprint> weapons,
            ReadonlyBlueprintCollection<IProjectileBlueprint> projectiles)
        {
            Sprites = sprites;
            Footprints = footprints;
            Buildings = buildings;
            Units = units;
            Weapons = weapons;
            Projectiles = projectiles;
        }

        public static Blueprints Merge(IEnumerable<Blueprints> blueprints)
        {
            var list = blueprints as IList<Blueprints> ?? blueprints.ToList();
            return new Blueprints(
                flatten(list, b => b.Sprites),
                flatten(list, b => b.Footprints),
                flatten(list, b => b.Buildings),
                flatten(list, b => b.Units),
                flatten(list, b => b.Weapons),
                flatten(list, b => b.Projectiles)
                );
        }

        private static ReadonlyBlueprintCollection<T> flatten<T>(
            IList<Blueprints> blueprints,
            Func<Blueprints, ReadonlyBlueprintCollection<T>> selector)
            where T : IBlueprint
        {
            return new ReadonlyBlueprintCollection<T>(blueprints.SelectMany(b => selector(b).All));
        }
    }
}

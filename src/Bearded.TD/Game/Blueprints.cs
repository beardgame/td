using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game
{
    sealed class Blueprints
    {
        public ReadonlyBlueprintCollection<Shader> Shaders { get; }
        public ReadonlyBlueprintCollection<Material> Materials { get; }
        public ReadonlyBlueprintCollection<SpriteSet> Sprites { get; }
        public ReadonlyBlueprintCollection<FootprintGroup> Footprints { get; }
        public ReadonlyBlueprintCollection<IBuildingBlueprint> Buildings { get; }
        public ReadonlyBlueprintCollection<IUnitBlueprint> Units { get; }
        public ReadonlyBlueprintCollection<IComponentOwnerBlueprint> ComponentOwners { get; }
        public ReadonlyBlueprintCollection<IUpgradeBlueprint> Upgrades { get; }
        public ReadonlyBlueprintCollection<ITechnologyBlueprint> Technologies { get; }
        public ReadonlyBlueprintCollection<IGameModeBlueprint> GameModes { get; }

        public Blueprints(ReadonlyBlueprintCollection<Shader> shaders,
            ReadonlyBlueprintCollection<Material> materials,
            ReadonlyBlueprintCollection<SpriteSet> sprites,
            ReadonlyBlueprintCollection<FootprintGroup> footprints,
            ReadonlyBlueprintCollection<IBuildingBlueprint> buildings,
            ReadonlyBlueprintCollection<IUnitBlueprint> units,
            ReadonlyBlueprintCollection<IComponentOwnerBlueprint> componentOwners,
            ReadonlyBlueprintCollection<IUpgradeBlueprint> upgrades,
            ReadonlyBlueprintCollection<ITechnologyBlueprint> technologies,
            ReadonlyBlueprintCollection<IGameModeBlueprint> gameModes)
        {
            Shaders = shaders;
            Materials = materials;
            Sprites = sprites;
            Footprints = footprints;
            Buildings = buildings;
            Units = units;
            ComponentOwners = componentOwners;
            Upgrades = upgrades;
            Technologies = technologies;
            GameModes = gameModes;
        }

        public static Blueprints Merge(IEnumerable<Blueprints> blueprints)
        {
            var list = blueprints as IList<Blueprints> ?? blueprints.ToList();
            return new Blueprints(
                flatten(list, b => b.Shaders),
                flatten(list, b => b.Materials),
                flatten(list, b => b.Sprites),
                flatten(list, b => b.Footprints),
                flatten(list, b => b.Buildings),
                flatten(list, b => b.Units),
                flatten(list, b => b.ComponentOwners),
                flatten(list, b => b.Upgrades),
                flatten(list, b => b.Technologies),
                flatten(list, b => b.GameModes)
                );
        }

        private static ReadonlyBlueprintCollection<T> flatten<T>(
            IEnumerable<Blueprints> blueprints,
            Func<Blueprints, ReadonlyBlueprintCollection<T>> selector)
            where T : IBlueprint
        {
            return new(blueprints.SelectMany(b => selector(b).All));
        }
    }
}

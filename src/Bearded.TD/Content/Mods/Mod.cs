using System.Collections.Generic;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Rules;
using Bearded.TD.Game.Technologies;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Game.World;

namespace Bearded.TD.Content.Mods
{
    sealed class Mod
    {
        public string Id { get; }
        public string Name { get; }

        public Blueprints Blueprints { get; }
        public IDictionary<string, UpgradeTag> Tags { get; }

        public Mod(string id,
            string name,
            ReadonlyBlueprintCollection<Shader> shaders,
            ReadonlyBlueprintCollection<Material> materials,
            ReadonlyBlueprintCollection<SpriteSet> sprites,
            ReadonlyBlueprintCollection<FootprintGroup> footprints,
            ReadonlyBlueprintCollection<IBuildingBlueprint> buildings,
            ReadonlyBlueprintCollection<IUnitBlueprint> units,
            ReadonlyBlueprintCollection<IComponentOwnerBlueprint> weapons,
            ReadonlyBlueprintCollection<IUpgradeBlueprint> upgrades,
            ReadonlyBlueprintCollection<ITechnologyBlueprint> technologies,
            ReadonlyBlueprintCollection<IGameModeBlueprint> gameModes,
            IDictionary<string, UpgradeTag> tags)
        {
            Id = id;
            Name = name;
            Blueprints = new Blueprints(
                shaders, materials, sprites, footprints, buildings, units, weapons, upgrades, technologies, gameModes);
            Tags = tags;
        }
    }
}

using System.Collections.Generic;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Content.Mods
{
    sealed class Mod
    {
        public string Id { get; }
        public string Name { get; }

        public Blueprints Blueprints { get; }
        public IDictionary<ModAwareId, UpgradeTag> Tags { get; }

        public Mod(string id,
            string name,
            ReadonlyBlueprintCollection<Shader> shaders,
            ReadonlyBlueprintCollection<Material> materials,
            ReadonlyBlueprintCollection<SpriteSet> sprites,
            ReadonlyBlueprintCollection<FootprintGroup> footprints,
            ReadonlyBlueprintCollection<IUnitBlueprint> units,
            ReadonlyBlueprintCollection<IComponentOwnerBlueprint> weapons,
            ReadonlyBlueprintCollection<IUpgradeBlueprint> upgrades,
            ReadonlyBlueprintCollection<ITechnologyBlueprint> technologies,
            ReadonlyBlueprintCollection<INodeBlueprint> levelNodes,
            ReadonlyBlueprintCollection<IGameModeBlueprint> gameModes,
            IDictionary<ModAwareId, UpgradeTag> tags)
        {
            Id = id;
            Name = name;
            Blueprints = new Blueprints(
                shaders,
                materials,
                sprites,
                footprints,
                units,
                weapons,
                upgrades,
                technologies,
                levelNodes,
                gameModes);
            Tags = tags;
        }
    }
}

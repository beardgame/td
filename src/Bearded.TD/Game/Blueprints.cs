using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Audio;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Models.Fonts;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game;

sealed record Blueprints(
    ReadonlyBlueprintCollection<Shader> Shaders,
    ReadonlyBlueprintCollection<Material> Materials,
    ReadonlyBlueprintCollection<SpriteSet> Sprites,
    ReadonlyBlueprintCollection<Model> Models,
    ReadonlyBlueprintCollection<FontDefinition> FontDefinitions,
    ReadonlyBlueprintCollection<Font> Fonts,
    ReadonlyBlueprintCollection<ISoundEffect> SoundEffects,
    ReadonlyBlueprintCollection<IFootprint> Footprints,
    ReadonlyBlueprintCollection<IGameObjectBlueprint> GameObjects,
    ReadonlyBlueprintCollection<IPermanentUpgrade> Upgrades,
    ReadonlyBlueprintCollection<IModule> Modules,
    ReadonlyBlueprintCollection<ITechnologyBlueprint> Technologies,
    ReadonlyBlueprintCollection<INodeBlueprint> LevelNodes,
    ReadonlyBlueprintCollection<IBiome> Biomes,
    ReadonlyBlueprintCollection<IGameModeBlueprint> GameModes)
{
    public static Blueprints Merge(IEnumerable<Blueprints> blueprints)
    {
        var list = blueprints as IList<Blueprints> ?? blueprints.ToList();
        return new Blueprints(
            flatten(list, b => b.Shaders),
            flatten(list, b => b.Materials),
            flatten(list, b => b.Sprites),
            flatten(list, b => b.Models),
            flatten(list, b => b.FontDefinitions),
            flatten(list, b => b.Fonts),
            flatten(list, b => b.SoundEffects),
            flatten(list, b => b.Footprints),
            flatten(list, b => b.GameObjects),
            flatten(list, b => b.Upgrades),
            flatten(list, b => b.Modules),
            flatten(list, b => b.Technologies),
            flatten(list, b => b.LevelNodes),
            flatten(list, b => b.Biomes),
            flatten(list, b => b.GameModes)
        );
    }

    private static ReadonlyBlueprintCollection<T> flatten<T>(
        IEnumerable<Blueprints> blueprints,
        Func<Blueprints, ReadonlyBlueprintCollection<T>> selector)
        where T : IBlueprint
    {
        return new ReadonlyBlueprintCollection<T>(blueprints.SelectMany(b => selector(b).All));
    }
}

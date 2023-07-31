using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class BiomeLoader
    : BaseBlueprintLoader<IBiome, Biome, Void>
{
    protected override string RelativePath => "defs/biomes";

    protected override DependencySelector SelectDependency => m => m.Blueprints.Biomes;

    public BiomeLoader(BlueprintLoadingContext context) : base(context) { }
}

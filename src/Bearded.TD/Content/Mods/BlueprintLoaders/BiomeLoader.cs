using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class BiomeLoader(BlueprintLoadingContext context)
    : BaseBlueprintLoader<IBiome, Biome, Void>(context)
{
    protected override string RelativePath => "defs/biomes";
    protected override DependencySelector SelectDependency => m => m.Blueprints.Biomes;
}

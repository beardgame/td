using System.IO;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class BiomeLoader
    : BaseBlueprintLoader<IBiome, Biome, IDependencyResolver<Content.Models.Material>>
{
    protected override string RelativePath => "defs/biomes";

    protected override DependencySelector SelectDependency => m => m.Blueprints.Biomes;

    public BiomeLoader(BlueprintLoadingContext context) : base(context) { }

    protected override IDependencyResolver<Content.Models.Material> GetDependencyResolvers(FileInfo file)
    {
        return Context.FindDependencyResolver<Content.Models.Material>();
    }
}

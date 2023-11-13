using System.IO;
using Material = Bearded.TD.Content.Models.Material;
using MaterialJson = Bearded.TD.Content.Serialization.Models.Material;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class MaterialBlueprintLoader : BaseBlueprintLoader<Material, MaterialJson, (FileInfo, MaterialLoader)>
{
    private readonly MaterialLoader materialLoader;

    protected override string RelativePath => "gfx/materials";

    protected override DependencySelector? SelectDependency { get; } = mod => mod.Blueprints.Materials; 

    public MaterialBlueprintLoader(BlueprintLoadingContext context)
        : base(context)
    {
        materialLoader = new MaterialLoader();
    }

    protected override (FileInfo, MaterialLoader) GetDependencyResolvers(FileInfo file)
    {
        return (file, materialLoader);
    }
}

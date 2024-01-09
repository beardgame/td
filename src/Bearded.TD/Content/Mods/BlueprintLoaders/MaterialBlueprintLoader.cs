using System.IO;
using Material = Bearded.TD.Content.Models.Material;
using MaterialJson = Bearded.TD.Content.Serialization.Models.Material;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class MaterialBlueprintLoader(BlueprintLoadingContext context)
    : BaseBlueprintLoader<Material, MaterialJson, (FileInfo, MaterialLoader)>(context)
{
    protected override string RelativePath => "gfx/materials";
    protected override DependencySelector SelectDependency => mod => mod.Blueprints.Materials;

    private readonly MaterialLoader materialLoader = new();

    protected override (FileInfo, MaterialLoader) GetDependencyResolvers(FileInfo file)
    {
        return (file, materialLoader);
    }
}

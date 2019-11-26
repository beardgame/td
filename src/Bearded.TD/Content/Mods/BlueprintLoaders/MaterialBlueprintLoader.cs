using System.IO;
using Material = Bearded.TD.Content.Models.Material;
using MaterialJson = Bearded.TD.Content.Serialization.Models.Material;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    class MaterialBlueprintLoader : BaseBlueprintLoader<Material, MaterialJson, (FileInfo, MaterialLoader)>
    {
        private readonly MaterialLoader materialLoader;

        protected override string RelativePath => "gfx/materials";

        public MaterialBlueprintLoader(BlueprintLoadingContext context)
            : base(context)
        {
            materialLoader = new MaterialLoader(context.Context);
        }

        protected override (FileInfo, MaterialLoader) GetDependencyResolvers(FileInfo file)
        {
            return (file, materialLoader);
        }
    }
}

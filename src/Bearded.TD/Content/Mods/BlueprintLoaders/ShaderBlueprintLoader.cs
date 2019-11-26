using System.IO;
using Shader = Bearded.TD.Content.Models.Shader;
using ShaderJson = Bearded.TD.Content.Serialization.Models.Shader;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    class ShaderBlueprintLoader : BaseBlueprintLoader<Shader, ShaderJson, (FileInfo, ShaderLoader)>
    {
        private readonly ShaderLoader shaderLoader;

        protected override string RelativePath => "gfx/shaders";

        protected override DependencySelector SelectDependency => m => m.Blueprints.Shaders;

        public ShaderBlueprintLoader(BlueprintLoadingContext context)
            : base(context)
        {
            shaderLoader = new ShaderLoader(context.Context, context.Meta);
        }

        protected override (FileInfo, ShaderLoader) GetDependencyResolvers(FileInfo file)
        {
            return (file, shaderLoader);
        }
    }
}

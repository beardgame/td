using System.IO;
using Shader = Bearded.TD.Content.Models.Shader;
using ShaderJson = Bearded.TD.Content.Serialization.Models.Shader;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class ShaderBlueprintLoader(BlueprintLoadingContext context)
    : BaseBlueprintLoader<Shader, ShaderJson, (FileInfo, ShaderLoader)>(context)
{
    protected override string RelativePath => "gfx/shaders";
    protected override DependencySelector SelectDependency => m => m.Blueprints.Shaders;

    private readonly ShaderLoader shaderLoader = new(context.Context, context.Meta);

    protected override (FileInfo, ShaderLoader) GetDependencyResolvers(FileInfo file)
    {
        return (file, shaderLoader);
    }
}

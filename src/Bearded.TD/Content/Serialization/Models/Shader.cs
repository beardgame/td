using System.IO;
using Bearded.TD.Content.Mods;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class Shader : IConvertsTo<Content.Models.Shader, (FileInfo, ShaderLoader)>
{
    public string? Id { get; set; }
    public string? VertexShader { get; set; }
    public string? FragmentShader { get; set; }
    public string? TessControlShader { get; set; }
    public string? TessEvaluationShader { get; set; }
    public string? GeometryShader { get; set; }
    public string? ComputeShader { get; set; }

    public Content.Models.Shader ToGameModel(ModMetadata contextMeta, (FileInfo, ShaderLoader) resolver)
    {
        var (fileInfo, shaderLoader) = resolver;
        return shaderLoader.TryLoad(fileInfo, this);
    }
}

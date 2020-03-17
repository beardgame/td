using System.IO;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Content.Serialization.Models
{
    class Shader : IConvertsTo<Content.Models.Shader, (FileInfo, ShaderLoader)>
    {
        public string? Id { get; set; }
        public string? VertexShader { get; set; }
        public string? FragmentShader { get; set; }

        public Content.Models.Shader ToGameModel((FileInfo, ShaderLoader) resolver)
        {
            var (fileInfo, shaderLoader) = resolver;
            return shaderLoader.TryLoad(fileInfo, this);
        }
    }
}

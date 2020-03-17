using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Content.Serialization.Models
{
    class Material : IConvertsTo<Content.Models.Material, (FileInfo, MaterialLoader)>
    {
        public string? Id { get; set; }

        public Content.Models.Shader? Shader { get; set; }

        public List<NamedTextureFileArray>? TextureArrays { get; set; }

        public Content.Models.Material ToGameModel((FileInfo, MaterialLoader) resolvers)
        {
            var (file, loader) = resolvers;
            var textures = (TextureArrays ?? Enumerable.Empty<NamedTextureFileArray>())
                .Select(array => (array.Name!, loader.CreateArrayTexture(file, array.Files)))
                .ToList().AsReadOnly();

            return new Content.Models.Material(Id!, Shader!, textures);
        }

        public class NamedTextureFileArray
        {
            public string? Name { get; set; }

            public List<string> Files { get; set; } = new List<string>();
        }
    }
}

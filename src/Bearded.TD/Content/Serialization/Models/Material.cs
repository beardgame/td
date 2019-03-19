using System.Collections.Generic;
using System.IO;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Content.Serialization.Models
{
    class Material : IConvertsTo<Content.Models.Material, (FileInfo, MaterialLoader)>
    {
        public string Id { get; set; }
        
        public Content.Models.Shader Shader { get; set; }
        
        public List<string> Textures { get; set; }

        public Content.Models.Material ToGameModel((FileInfo, MaterialLoader) resolvers)
        {
            var (file, loader) = resolvers;
            var texture = loader.CreateArrayTexture(file, this);
            
            return new Content.Models.Material(Id, Shader, texture);
        }
    }
}

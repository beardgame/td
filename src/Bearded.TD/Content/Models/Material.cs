using System.Collections.ObjectModel;
using amulware.Graphics;
using Bearded.TD.Game;

namespace Bearded.TD.Content.Models
{
    class Material : IBlueprint
    {
        public string Id { get; }
        public Shader Shader { get; }
        public ReadOnlyCollection<(string UniformName, ArrayTexture Texture)> ArrayTextures { get; }
        
        public Material(string id, Shader shader, ReadOnlyCollection<(string, ArrayTexture)> arrayTextures)
        {
            Id = id;
            Shader = shader;
            ArrayTextures = arrayTextures;
        }
    }
}

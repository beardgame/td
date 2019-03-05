using amulware.Graphics;
using Bearded.TD.Game;

namespace Bearded.TD.Content.Models
{
    class Material : IBlueprint
    {
        public string Id { get; }
        public Shader Shader { get; }
        public Texture TextureArray { get; }
        
        public Material(string id, Shader shader, Texture textureArray)
        {
            Id = id;
            Shader = shader;
            TextureArray = textureArray;
        }
    }
}

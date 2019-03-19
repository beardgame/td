using amulware.Graphics;
using Bearded.TD.Game;

namespace Bearded.TD.Content.Models
{
    class Material : IBlueprint
    {
        public string Id { get; }
        public Shader Shader { get; }
        public ArrayTexture ArrayTexture { get; }
        
        public Material(string id, Shader shader, ArrayTexture arrayTexture)
        {
            Id = id;
            Shader = shader;
            ArrayTexture = arrayTexture;
        }
    }
}

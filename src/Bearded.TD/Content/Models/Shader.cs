using amulware.Graphics;
using Bearded.TD.Game;

namespace Bearded.TD.Content.Models
{
    class Shader : IBlueprint
    {
        public string Id { get; }

        public ISurfaceShader SurfaceShader { get; }

        public Shader(string id, ISurfaceShader surfaceShader)
        {
            Id = id;
            SurfaceShader = surfaceShader;
        }

    }
}

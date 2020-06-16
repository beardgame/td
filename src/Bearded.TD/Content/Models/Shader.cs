using amulware.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;

namespace Bearded.TD.Content.Models
{
    sealed class Shader : IBlueprint
    {
        public ModAwareId Id { get; }

        public ISurfaceShader SurfaceShader { get; }

        public Shader(ModAwareId id, ISurfaceShader surfaceShader)
        {
            Id = id;
            SurfaceShader = surfaceShader;
        }

    }
}

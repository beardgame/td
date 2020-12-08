using System.Collections.ObjectModel;
using amulware.Graphics.Textures;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.GameState;

namespace Bearded.TD.Content.Models
{
    sealed class Material : IBlueprint
    {
        public ModAwareId Id { get; }
        public Shader Shader { get; }
        public ReadOnlyCollection<(string UniformName, ArrayTexture Texture)> ArrayTextures { get; }

        public Material(ModAwareId id, Shader shader, ReadOnlyCollection<(string, ArrayTexture)> arrayTextures)
        {
            Id = id;
            Shader = shader;
            ArrayTextures = arrayTextures;
        }
    }
}

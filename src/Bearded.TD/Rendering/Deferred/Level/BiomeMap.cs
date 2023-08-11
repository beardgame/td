using Bearded.TD.Game;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering.Deferred.Level;

internal sealed class BiomeMap : LevelTextureMap
{
    public BiomeMap(GameInstance game)
        : base(game, "biomeMap",
            //
            PixelInternalFormat.R8)
    {
    }
}

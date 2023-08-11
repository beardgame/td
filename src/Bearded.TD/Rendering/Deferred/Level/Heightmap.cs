using Bearded.Graphics.Pipelines;
using Bearded.TD.Game;
using OpenTK.Graphics.OpenGL;
using static Bearded.Graphics.Pipelines.Context.ColorMask;

namespace Bearded.TD.Rendering.Deferred.Level;

sealed class Heightmap : LevelTextureMap
{
    public Heightmap(GameInstance game)
        : base(game, "heightmap",
            // Format: R: Height, G: Visibility
            PixelInternalFormat.Rg16f)
    {

    }

    public IPipeline<T> DrawHeights<T>(IPipeline<T> render)
        => DrawWithMask(render, DrawRed);

    public IPipeline<T> DrawVisibility<T>(IPipeline<T> render)
        => DrawWithMask(render, DrawGreen);
}

using Bearded.Graphics.Textures;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Deferred.Level;

namespace Bearded.TD.Rendering;

sealed class HeightmapUniformValues(Heightmap heightmap)
{
    public float Radius => heightmap.RadiusUniform.Value;
    public float PixelSizeUV => heightmap.PixelSizeUVUniform.Value;
    public Texture Texture => heightmap.Texture;
}

sealed class DeferredContent(LevelRenderer levelRenderer, IDrawableRenderers renderers)
{
    public HeightmapUniformValues Heightmap { get; } = new(levelRenderer.Heightmap);

    public void RenderDrawGroup(DrawOrderGroup group)
    {
        renderers.RenderDrawGroup(group);
    }
}

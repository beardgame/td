using System.Linq;
using Bearded.Graphics.Textures;

namespace Bearded.TD.Rendering.Loading;

static class SpriteTextureTransformations
{
    public static ITextureTransformation DoNothing { get; } = new SpriteTextureTransformation(_ => { });

    public static ITextureTransformation Test { get; } = new SpriteTextureTransformation(
        data =>
        {
            var green = data.Green;
            foreach (var y in Enumerable.Range(0, data.Height))
            foreach (var x in Enumerable.Range(0, data.Width))
            {
                green[x, y] = 255;
            }
        }
    );

    public static ITextureTransformation NormalFromHeight { get; } = new SpriteTextureTransformation(
        data => new NormalMapGenerator(data).Generate());
}

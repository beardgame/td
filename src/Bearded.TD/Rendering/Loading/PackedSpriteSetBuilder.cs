using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics.ImageSharp;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Textures;
using Bearded.TD.Content.Models;
using Bearded.Utilities.Threading;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Rectangle = System.Drawing.Rectangle;

namespace Bearded.TD.Rendering.Loading;

sealed class PackedSpriteSetBuilder
{
    private readonly int width;
    private readonly int height;
    private readonly Dictionary<string, Image<Bgra32>> samplerData = new();
    private readonly Dictionary<string, Rectangle> nameToRectangle = new();

    public PackedSpriteSetBuilder(List<string> samplers, int width, int height)
    {
        this.width = width;
        this.height = height;
        foreach (var sampler in samplers)
        {
            samplerData[sampler] = new Image<Bgra32>(width, height);
        }
    }

    public void CopyBitmap(string sprite, string sampler, Image<Bgra32> image, int x, int y)
    {
        nameToRectangle[sprite] = new Rectangle(x, y, image.Width, image.Height);

        var data = samplerData[sampler];

        data.Mutate(c => c.DrawImage(image, new Point(x, y), 1));
    }

    public PackedSpriteSet Build(
        IActionQueue glActions,
        Dictionary<string, IEnumerable<ITextureTransformation>> transformationsBySampler)
    {
        var premultiply = TextureTransformation.Premultiply;

        var transformedSamplerImages = samplerData.Select(
            kvp =>
            {
                var (sampler, image) = kvp;

                var transformations = transformationsBySampler[sampler].Prepend(premultiply);

                return (sampler, ImageTextureData.From(image, transformations));
            }).ToList();

        var textureUniforms = glActions.Run(() => createGlEntities(transformedSamplerImages)).Result;
        var sprites = createSprites();

        return new PackedSpriteSet(
            textureUniforms,
            sprites
        );
    }

    private List<TextureUniform> createGlEntities(List<(string Name, ITextureData Data)> samplers)
    {
        var textureUniforms = samplers.Select((sampler, i) =>
        {
            var texture = Texture.From(sampler.Data, t =>
            {
                t.GenerateMipmap();
                t.SetFilterMode(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
            });

            return new TextureUniform(sampler.Name, TextureUnit.Texture0 + i, texture);
        }).ToList();

        return textureUniforms;
    }

    private Dictionary<string, SpriteParameters> createSprites()
    {
        return nameToRectangle.ToDictionary(
            pair => pair.Key,
            pair => createSprite(pair.Value)
        );
    }

    private SpriteParameters createSprite(Rectangle rectangle)
    {
        var uv = toUvRectangle(rectangle);

        return new SpriteParameters(uv, new Vector2(1, rectangle.Height / (float)rectangle.Width));
    }

    private UVRectangle toUvRectangle(Rectangle rect)
    {
        return new (
            (float)rect.Left / width, (float)rect.Right / width,
            (float)rect.Top / height, (float)rect.Bottom / height);
    }
}

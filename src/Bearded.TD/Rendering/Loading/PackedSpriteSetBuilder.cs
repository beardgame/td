using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Textures;
using Bearded.TD.Content.Models;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Threading;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Bearded.TD.Rendering.Loading;

class PackedSpriteSetBuilder
{
    private readonly int width;
    private readonly int height;
    private readonly Dictionary<string, byte[]> samplerData = new();
    private readonly Dictionary<string, Rectangle> nameToRectangle = new();

    public PackedSpriteSetBuilder(List<string> samplers, int width, int height)
    {
        this.width = width;
        this.height = height;
        foreach (var sampler in samplers)
        {
            samplerData[sampler] = new byte[width * height * 4];
        }
    }

    public void CopyBitmap(string sprite, string sampler, Bitmap bitmap, int x, int y)
    {
        nameToRectangle[sprite] = new Rectangle(x, y, bitmap.Width, bitmap.Height);

        var data = samplerData[sampler];

        var bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

        var scan0 = bitmapData.Scan0;
        var stride = bitmapData.Stride;

        for (var i = 0; i < bitmap.Height; i++)
        {
            var start = scan0 + stride * i;
            Marshal.Copy(
                start,
                data,
                flatCoordinate(x, y + i),
                stride);
        }

        bitmap.UnlockBits(bitmapData);
    }

    private int flatCoordinate(int x, int y)
        => 4 * (y * width + x);

    public PackedSpriteSet Build(
        IActionQueue glActions,
        bool pixelate,
        Dictionary<string, IEnumerable<ITextureTransformation>> transformationsBySampler)
    {
        var premultiply = TextureTransformation.Premultiply;

        samplerData.ForEach(kvp =>
        {
            var (sampler, bytes) = kvp;
            var (w, h) = (width, height);
            premultiply.Transform(ref bytes, ref w, ref h);
            foreach (var transform in transformationsBySampler[sampler])
            {
                transform.Transform(ref bytes, ref w, ref h);
            }
        });

        var textureUniforms = glActions.Run(() => createGlEntities(pixelate)).Result;
        var sprites = createSprites();

        return new PackedSpriteSet(
            textureUniforms,
            sprites
        );
    }

    private List<TextureUniform> createGlEntities(bool pixelate)
    {
        var textureUniforms = samplerData.Select(
            (kvp, i) =>
            {
                var (name, data) = kvp;
                var textureData = RawTextureData.From(data, width, height);
                var texture = Texture.From(textureData, t =>
                {
                    if (pixelate)
                    {
                        t.SetFilterMode(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
                        t.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
                    }
                    else
                    {
                        t.GenerateMipmap();
                    }
                });

                return new TextureUniform(name, TextureUnit.Texture0 + i, texture);
            }
        ).ToList();

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

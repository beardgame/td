using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Algorithms;
using Bearded.Utilities.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Shader = Bearded.TD.Content.Models.Shader;

namespace Bearded.TD.Rendering.Loading
{
    class PackedSpriteSetBuilder
    {
        private readonly int width;
        private readonly int height;
        private readonly Dictionary<string, byte[]> samplerData = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, Rectangle> nameToRectangle = new Dictionary<string, Rectangle>();

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

        public PackedSpriteSet Build(RenderContext context, IActionQueue glActions, Shader shader, bool pixelate)
        {
            samplerData.Values.ForEach(Texture.PreMultipleArgbArray);

            var (textureUniforms, surface) = glActions.RunAndReturn(() => createGlEntities(shader, pixelate));
            var sprites = createSprites(surface);

            surface.AddSettings(
                context.Surfaces.ProjectionMatrix,
                context.Surfaces.ViewMatrix,
                context.Surfaces.FarPlaneDistance
            );
            surface.AddSettings(
                textureUniforms
            );

            return new PackedSpriteSet(
                textureUniforms.Select(u => u.Texture),
                surface,
                sprites
            );
        }

        private Dictionary<string, ISprite> createSprites(IndexedSurface<UVColorVertexData> surface)
        {
            return nameToRectangle.ToDictionary(
                pair => pair.Key,
                pair => createSprite(surface, pair.Value)
            );
        }

        private ISprite createSprite(IndexedSurface<UVColorVertexData> surface, Rectangle rectangle)
        {
            var uv = toUvRectangle(rectangle);

            return new Sprite(surface, uv, new Vector2(rectangle.Width, rectangle.Height) * Constants.Rendering.PixelSize);
        }

        private UVRectangle toUvRectangle(Rectangle rect)
        {
            return new UVRectangle(
                (float)rect.Left / width, (float)rect.Right / width,
                (float)rect.Top / height, (float)rect.Bottom / height);
        }

        private (List<TextureUniform>, IndexedSurface<UVColorVertexData>) createGlEntities(Shader shader, bool pixelate)
        {
            var textureUniforms = samplerData.Select(
                (kvp, i) =>
                {
                    var (name, data) = kvp;
                    var texture = new Texture(data, width, height);
                    if (pixelate)
                    {
                        texture.SetParameters(
                            TextureMinFilter.Nearest, TextureMagFilter.Nearest,
                            TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge
                        );
                    }

                    return new TextureUniform(name, texture, TextureUnit.Texture0 + i);
                }
            ).ToList();

            var surface = new IndexedSurface<UVColorVertexData>();

            shader.SurfaceShader.UseOnSurface(surface);

            return (textureUniforms, surface);
        }
    }
}

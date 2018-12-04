using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using amulware.Graphics;
using Bearded.TD.Mods;
using Bearded.TD.Mods.Models;
using Bearded.Utilities.Threading;

namespace Bearded.TD.Rendering.Loading
{
    partial class GraphicsLoader : IGraphicsLoader
    {
        private class PackedSpriteSetBuilder
        {
            private readonly int width;
            private readonly int height;
            private readonly byte[] data;
            private readonly Dictionary<string, Rectangle> nameToRectangle = new Dictionary<string, Rectangle>();

            public PackedSpriteSetBuilder(int width, int height)
            {
                this.width = width;
                this.height = height;
                data = new byte[width * height * 4];
            }

            public void CopyBitmap((Bitmap Image, string Name) tuple, int x, int y)
            {
                nameToRectangle.Add(tuple.Name, new Rectangle(x, y, tuple.Image.Width, tuple.Image.Height));

                var bitmapData = tuple.Image.LockBits(
                    new Rectangle(0, 0, tuple.Image.Width, tuple.Image.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                var scan0 = bitmapData.Scan0;
                var stride = bitmapData.Stride;

                for (var i = 0; i < tuple.Image.Height; i++)
                {
                    var start = scan0 + stride * i;
                    Marshal.Copy(
                        start,
                        data,
                        flatCoordinate(x, y + i),
                        stride);
                }

                tuple.Image.UnlockBits(bitmapData);
            }

            private int flatCoordinate(int x, int y)
                => 4 * (y * width + x);

            public PackedSpriteSet Build(RenderContext context, IActionQueue glActions)
            {
                Texture.PreMultipleArgbArray(data);

                var (texture, surface) = glActions.RunAndReturn(() => createGlEntities(context));
                var sprites = createSprites(surface);

                return new PackedSpriteSet(
                    texture,
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

                return new Sprite(surface, uv);
            }

            private UVRectangle toUvRectangle(Rectangle rect)
            {
                return new UVRectangle(
                    (float)rect.Left / width, (float)rect.Right / width,
                    (float)rect.Top / height, (float)rect.Bottom / height);
            }

            private (Texture, IndexedSurface<UVColorVertexData>) createGlEntities(RenderContext context)
            {
                var texture = new Texture(data, width, height);

                var surface = new IndexedSurface<UVColorVertexData>()
                    .WithShader(context.Surfaces.Shaders["uvcolor"])
                    .AndSettings(
                        context.Surfaces.ProjectionMatrix,
                        context.Surfaces.ViewMatrix,
                        new TextureUniform("diffuseTexture", texture)
                        );

                return (texture, surface);
            }
        }
    }
}

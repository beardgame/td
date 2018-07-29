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

                for (var i = 0; i < tuple.Image.Height; i++)
                {
                    Marshal.Copy(
                        bitmapData.Scan0 + bitmapData.Stride,
                        data,
                        flatCoordinate(x, y + i),
                        bitmapData.Stride);
                }

                tuple.Image.UnlockBits(bitmapData);
            }

            private int flatCoordinate(int x, int y)
                => 4 * (y * width + x);

            public PackedSpriteSet Build(IActionQueue glActions)
            {
                var texture = glActions.RunAndReturn(createGlEntities);
                var spriteUVs = createSpriteUVs();
                return new PackedSpriteSet(
                    texture,
                    spriteUVs
                    );
            }

            private Dictionary<string, UVRectangle> createSpriteUVs()
            {
                return nameToRectangle.ToDictionary(
                    pair => pair.Key,
                    pair => toUvRectangle(pair.Value)
                );
            }

            private UVRectangle toUvRectangle(Rectangle rect)
            {
                return new UVRectangle(
                    (float)rect.Left / width, (float)rect.Right / width,
                    (float)rect.Top / width, (float)rect.Bottom / width);
            }

            private Texture createGlEntities()
            {
                return new Texture(data, width, height);
            }
        }
    }
}

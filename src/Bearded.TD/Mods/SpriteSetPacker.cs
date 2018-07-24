using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using amulware.Graphics;
using Bearded.TD.Mods.Models;
using Bearded.Utilities.Algorithms;

namespace Bearded.TD.Mods
{
    sealed class SpriteSetPacker
    {
        public PackedSpriteSet LoadPackedSpriteSet(DirectoryInfo directory, ModLoadingContext modLoadingContext)
        {
            var bitmaps = annotatedPngFilesInRecursive(directory)
                .Select(tuple => (name: tuple.name, bitmap: new Bitmap(tuple.file.OpenRead()))).ToList();
            var binPackingResult = BinPacking.Pack(
                bitmaps.Select(tuple
                    => new BinPacking.Rectangle<(string name, Bitmap bitmap)>(
                        tuple, tuple.bitmap.Width, tuple.bitmap.Height)));

            var spriteSetBuilder = new PackedSpriteSetBuilder(binPackingResult.Width, binPackingResult.Height);

            foreach (var rectangle in binPackingResult.Rectangles)
            {
                spriteSetBuilder.CopyBitmap(rectangle.Value, rectangle.X, rectangle.Y);
                rectangle.Value.bitmap.Dispose();
            }

            return spriteSetBuilder.Build(modLoadingContext);
        }

        private static IEnumerable<(string name, FileInfo file)> annotatedPngFilesInRecursive(
            DirectoryInfo directory, string prefix = "")
        {
            foreach (var file in pngFilesInFlat(directory))
                yield return (prefix + Path.GetFileNameWithoutExtension(file.Name), file);

            var newPrefix = prefix + "." + directory.Name;

            foreach (var dir in directory.GetDirectories("*", SearchOption.TopDirectoryOnly))
            foreach (var tuple in annotatedPngFilesInRecursive(dir, newPrefix))
                yield return tuple;
        }

        private static IEnumerable<FileInfo> pngFilesInFlat(DirectoryInfo directory)
            => directory.GetFiles("*.png", SearchOption.TopDirectoryOnly);

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

            public void CopyBitmap((string name, Bitmap bitmap) tuple, int x, int y)
            {
                nameToRectangle.Add(tuple.name, new Rectangle(x, y, tuple.bitmap.Width, tuple.bitmap.Height));

                var bitmapData = tuple.bitmap.LockBits(
                    new Rectangle(0, 0, tuple.bitmap.Width, tuple.bitmap.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                for (var i = 0; i < tuple.bitmap.Height; i++)
                {
                    Marshal.Copy(
                        bitmapData.Scan0 + bitmapData.Stride,
                        data,
                        flatCoordinate(x, y + i),
                        bitmapData.Stride);
                }

                tuple.bitmap.UnlockBits(bitmapData);
            }

            private int flatCoordinate(int x, int y)
                => 4 * (y * width + x);

            public PackedSpriteSet Build(ModLoadingContext modLoadingContext)
            {
                return new PackedSpriteSet(
                    getTexture(modLoadingContext),
                    nameToRectangle.ToDictionary(pair => pair.Key, pair => toUvRectangle(pair.Value)));
            }

            private UVRectangle toUvRectangle(Rectangle rect)
            {
                return new UVRectangle(
                    (float)rect.Left / width, (float)rect.Right / width,
                    (float)rect.Top / width, (float)rect.Bottom / width);
            }

            private Texture getTexture(ModLoadingContext modLoadingContext)
            {
                return modLoadingContext.GlActions.RunAndReturn(() => new Texture(data, width, height));
            }
        }
    }
}

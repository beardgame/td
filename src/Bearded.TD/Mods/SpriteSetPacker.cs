using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using amulware.Graphics;
using Bearded.Utilities.Algorithms;

namespace Bearded.TD.Mods
{
    sealed class SpriteSetPacker
    {
        public Texture GetPackedTextureForSpriteSet(DirectoryInfo directory)
        {
            var bitmaps = pngFilesIn(directory).Select(file => new Bitmap(file.OpenRead())).ToList();
            var binPackingResult = BinPacking.Pack(
                bitmaps.Select(bitmap => new BinPacking.Rectangle<Bitmap>(bitmap, bitmap.Width, bitmap.Height)));

            var packedBitmap = new PackedSpriteSetBitmap(binPackingResult.Width, binPackingResult.Height);

            foreach (var rectangle in binPackingResult.Rectangles)
            {
                packedBitmap.CopyBitmap(rectangle.Value, rectangle.X, rectangle.Y);
                rectangle.Value.Dispose();
            }

            // TODO: write data array to texture
            return null;
        }

        private static IEnumerable<FileInfo> pngFilesIn(DirectoryInfo directory)
            => directory.GetFiles("*.png", SearchOption.AllDirectories);

        private class PackedSpriteSetBitmap
        {
            private readonly int width;
            private readonly byte[] data;

            public PackedSpriteSetBitmap(int width, int height)
            {
                this.width = width;
                data = new byte[width * height * 4];
            }

            public void CopyBitmap(Bitmap bitmap, int x, int y)
            {
                var bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                for (var i = 0; i < bitmap.Height; i++)
                {
                    Marshal.Copy(
                        bitmapData.Scan0 + bitmapData.Stride,
                        data,
                        flatCoordinate(x, y + i),
                        bitmapData.Stride);
                }

                bitmap.UnlockBits(bitmapData);
            }

            private int flatCoordinate(int x, int y)
                => 4 * (y * width + x);
        }
    }
}

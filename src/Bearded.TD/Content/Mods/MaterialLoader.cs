using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Bearded.Graphics.Textures;

namespace Bearded.TD.Content.Mods
{
    class MaterialLoader
    {
        private readonly ModLoadingContext context;

        public MaterialLoader(ModLoadingContext context)
        {
            this.context = context;
        }

        public ArrayTexture CreateArrayTexture(FileInfo file, IEnumerable<string> textureFilenames)
        {
            var baseDir = file.Directory ?? throw new InvalidOperationException("Directory should never be null");

            var textureFiles = textureFilenames
                .Select(name => (Name: name, File: baseDir.GetFiles(name).SingleOrDefault()))
                .Where(f => f.File != null ? true : throw new InvalidDataException($"Could not find unique material texture file '{f.Name}'."))
                .Select(f => f.File)
                .ToList();

            var textureBitmaps = textureFiles.Select(f => new Bitmap(f.FullName)).ToList();

            var resizedBitmaps = unifySize(textureBitmaps);

            return context.GraphicsLoader.CreateArrayTexture(resizedBitmaps);
        }

        private static List<Bitmap> unifySize(List<Bitmap> bitmaps)
        {
            var size = new Size(bitmaps.Max(b => b.Width), bitmaps.Max(b => b.Height));

            return bitmaps.Select(b => b.Size == size ? b : new Bitmap(b, size)).ToList();
        }
    }
}

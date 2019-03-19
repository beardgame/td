using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using amulware.Graphics;

namespace Bearded.TD.Content.Mods
{
    class MaterialLoader
    {
        private readonly ModLoadingContext context;
        private readonly ModMetadata meta;

        public MaterialLoader(ModLoadingContext context, ModMetadata meta)
        {
            this.context = context;
            this.meta = meta;
        }

        public ArrayTexture CreateArrayTexture(FileInfo file, Serialization.Models.Material materialJson)
        {
            var baseDir = file.Directory;
            var textureFiles = materialJson.Textures
                .Select(name => baseDir.GetFiles(name).SingleOrDefault())
                .Where(f => f != null ? true : throw new InvalidDataException($"Could not find material texture file '{f}'."))
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

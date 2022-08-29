using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.Graphics.Textures;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using Image = SixLabors.ImageSharp.Image;

namespace Bearded.TD.Content.Mods;

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

        var images = textureFiles.Select(f => SKBitmap.Decode(f.FullName)).ToList();

        var resizedImages = unifySize(images);

        return context.GraphicsLoader.CreateArrayTexture(resizedImages);
    }

    private static List<SKBitmap> unifySize(List<SKBitmap> images)
    {
        var width = images.Max(i => i.Width);
        var height = images.Max(i => i.Height);
        return images.Select(image =>
        {
            if (image.Width == width && image.Height == height)
            {
                return image;
            }

            return image.Resize(new SKSizeI(width, height), SKFilterQuality.High);
        }).ToList();
    }

}

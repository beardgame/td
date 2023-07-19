using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.Graphics.Textures;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace Bearded.TD.Content.Mods;

sealed class MaterialLoader
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

        var images = textureFiles.Select(f => Image.Load(f.FullName)).ToList();

        unifySize(images);

        return context.GraphicsLoader.CreateArrayTexture(images);
    }

    private static void unifySize(List<Image> images)
    {
        var width = images.Max(i => i.Width);
        var height = images.Max(i => i.Height);
        foreach (var image in images.Where(i => i.Width != width || i.Height != height))
        {
            image.Mutate(i => i.Resize(width, height));
        }
    }

}

using System;
using System.IO;
using System.Linq;
using Image = SixLabors.ImageSharp.Image;

namespace Bearded.TD.Content.Mods;

sealed class MaterialLoader
{
    public Image LoadTextureImage(FileInfo file, string textureFilename)
    {
        var baseDir = file.Directory ?? throw new InvalidOperationException("Directory should never be null");

        var textureFile = baseDir.GetFiles(textureFilename).SingleOrDefault()
            ?? throw new InvalidDataException($"Could not find unique material texture file '{textureFilename}'.");

        var image = Image.Load(textureFile.FullName);

        return image;
    }
}

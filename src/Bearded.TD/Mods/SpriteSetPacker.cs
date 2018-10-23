using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Mods
{
    sealed class SpriteSetPacker
    {
        public PackedSpriteSet LoadPackedSpriteSet(DirectoryInfo directory, ModLoadingContext modLoadingContext)
        {
            var files = annotatedPngFilesInRecursive(directory);

            var bitmaps = files.Select(file => (new Bitmap(file.file.OpenRead()), file.name));

            return modLoadingContext.GraphicsLoader.CreateSpriteSet(bitmaps);
        }

        private static IEnumerable<(string name, FileInfo file)> annotatedPngFilesInRecursive(
            DirectoryInfo directory, string prefix = "")
        {
            foreach (var file in pngFilesInFlat(directory))
                yield return (prefix + Path.GetFileNameWithoutExtension(file.Name), file);

            var newPrefix = prefix + "." + directory.Name;

            foreach (var dir in directory.GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                foreach (var tuple in annotatedPngFilesInRecursive(dir, newPrefix))
                    yield return tuple;
            }
        }

        private static IEnumerable<FileInfo> pngFilesInFlat(DirectoryInfo directory)
            => directory.GetFiles("*.png", SearchOption.TopDirectoryOnly);
    }
}

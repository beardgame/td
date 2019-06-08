using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Models;

namespace Bearded.TD.Content.Mods
{
    internal class SpriteSetLoader
    {
        private readonly ModLoadingContext context;

        public SpriteSetLoader(ModLoadingContext context)
        {
            this.context = context;
        }

        public SpriteSet TryLoad(FileInfo file, Content.Serialization.Models.SpriteSet jsonModel)
        {
            var packedSpriteSet = loadPackedSpriteSet(file.Directory, context, jsonModel);

            return new SpriteSet(jsonModel.Id, jsonModel.DrawGroup, jsonModel.DrawGroupOrderKey, packedSpriteSet);
        }

        private static PackedSpriteSet loadPackedSpriteSet(DirectoryInfo directory, ModLoadingContext modLoadingContext,
            Serialization.Models.SpriteSet jsonModel)
        {
            var files = annotatedPngFilesInRecursive(directory);

            var bitmaps = files.Select(file => (new Bitmap(file.file.OpenRead()), file.name));

            return modLoadingContext.GraphicsLoader.CreateSpriteSet(bitmaps, jsonModel.Shader, jsonModel.DefaultTextureSampler);
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

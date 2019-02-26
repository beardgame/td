using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Models;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Mods
{
    internal class SpriteSetLoader
    {
        private readonly ModLoadingContext context;
        private readonly ModMetadata meta;
        private readonly JsonSerializer serializer;

        public SpriteSetLoader(ModLoadingContext context, ModMetadata meta, JsonSerializer serializer)
        {
            this.context = context;
            this.meta = meta;
            this.serializer = serializer;
        }

        public SpriteSet TryLoad(FileInfo file)
        {
            try
            {
                var text = file.OpenText();
                var reader = new JsonTextReader(text);
                var jsonModel = serializer.Deserialize<Serialization.Models.SpriteSet>(reader);

                var packedSpriteSet = loadPackedSpriteSet(file.Directory, context);

                var gameModel = jsonModel.ToGameModel(packedSpriteSet);

                return gameModel;
            }
            catch (Exception e)
            {
                context.Logger.Error?.Log($"Error loading '{meta.Id}/gfx/sprites/../{file.Name}': {e.Message}");

                return null;
            }
        }

        private static PackedSpriteSet loadPackedSpriteSet(DirectoryInfo directory, ModLoadingContext modLoadingContext)
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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Models;

namespace Bearded.TD.Content.Mods
{
    sealed class SpriteSetLoader
    {
        private readonly ModLoadingContext context;
        private readonly ModMetadata meta;

        public SpriteSetLoader(ModLoadingContext context, ModMetadata meta)
        {
            this.context = context;
            this.meta = meta;
        }

        public SpriteSet TryLoad(FileInfo file, Content.Serialization.Models.SpriteSet jsonModel)
        {
            _ = jsonModel.Id ?? throw new InvalidDataException($"{nameof(jsonModel.Id)} must not be null");

            var packedSpriteSet = loadPackedSpriteSet(
                file.Directory ?? throw new NullReferenceException(), context, jsonModel);

            return new SpriteSet(
                ModAwareId.FromNameInMod(jsonModel.Id, meta),
                jsonModel.DrawGroup,
                jsonModel.DrawGroupOrderKey,
                packedSpriteSet);
        }

        private static ISpriteSetImplementation loadPackedSpriteSet(DirectoryInfo directory, ModLoadingContext modLoadingContext,
            Serialization.Models.SpriteSet jsonModel)
        {
            _ = jsonModel.Id ?? throw new InvalidDataException($"{nameof(jsonModel.Id)} must be non-null");
            _ = jsonModel.Shader ?? throw new InvalidDataException($"{nameof(jsonModel.Shader)} must be non-null");

            var samplers = getSamplers(jsonModel);
            var sprites = sortFilesBySpriteAndSampler(directory, samplers);

            return modLoadingContext.GraphicsLoader.CreateSpriteSet(
                samplers.Select(s => s.name), sprites, jsonModel.Shader, jsonModel.Pixelate, jsonModel.Id);
        }

        private static IEnumerable<(string, Dictionary<string, Lazy<Bitmap>>)>
            sortFilesBySpriteAndSampler(
                DirectoryInfo directory,
                List<(string name, string prefix, string suffix)> samplers
            )
        {
            var sprites = new Dictionary<string, Dictionary<string, Lazy<Bitmap>>>();

            addPngFilesRecursively(directory, samplers, sprites, "");

            return sprites.Select(kvp => (kvp.Key, kvp.Value));
        }

        private static void addPngFilesRecursively(
            DirectoryInfo directory,
            List<(string name, string prefix, string suffix)> samplers,
            Dictionary<string, Dictionary<string, Lazy<Bitmap>>> sprites,
            string spriteNamePrefix
            )
        {
            foreach (var file in pngFilesInFlat(directory))
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);

                var (samplerName, prefix, suffix) = samplers
                    .First(s => fileNameWithoutExtension.StartsWith(s.prefix) &&
                        fileNameWithoutExtension.EndsWith(s.suffix));

                var localSpriteName = fileNameWithoutExtension.Substring(
                    prefix.Length,
                    fileNameWithoutExtension.Length - prefix.Length - suffix.Length);

                var spriteName = spriteNamePrefix + localSpriteName;

                if (!sprites.ContainsKey(spriteName))
                    sprites[spriteName] = new Dictionary<string, Lazy<Bitmap>>();

                sprites[spriteName][samplerName] = lazyReadBitmap(file);
            }

            foreach (var dir in directory.GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                var newPrefix = spriteNamePrefix + "." + dir.Name;

                addPngFilesRecursively(dir, samplers, sprites, newPrefix);
            }
        }

        private static Lazy<Bitmap> lazyReadBitmap(FileInfo file)
        {
            return new Lazy<Bitmap>(() =>
            {
                var fileStream = file.OpenRead();
                var bitmap = new Bitmap(fileStream);
                fileStream.Close();
                return bitmap;
            });
        }

        private static List<(string name, string prefix, string suffix)> getSamplers(Serialization.Models.SpriteSet jsonModel)
        {
            var foundDefaultSampler = false;

            var samplers = new List<(string, string, string)>();

            if (jsonModel.TextureSamplers != null)
            {
                foreach (var sampler in Enumerable.Reverse(jsonModel.TextureSamplers))
                {
                    if (string.IsNullOrWhiteSpace(sampler.Sampler))
                        throw new InvalidDataException("Texture sampler needs non empty 'sampler' property");

                    if (string.IsNullOrEmpty(sampler.Prefix) && string.IsNullOrEmpty(sampler.Suffix))
                        foundDefaultSampler = true;

                    samplers.Add((sampler.Sampler, sampler.Prefix ?? "", sampler.Suffix ?? ""));
                }
            }

            if (!foundDefaultSampler && !string.IsNullOrWhiteSpace(jsonModel.DefaultTextureSampler))
            {
                samplers.Add((jsonModel.DefaultTextureSampler, "", ""));
            }

            return samplers;
        }

        private static IEnumerable<FileInfo> pngFilesInFlat(DirectoryInfo directory)
            => directory.GetFiles("*.png", SearchOption.TopDirectoryOnly);
    }
}

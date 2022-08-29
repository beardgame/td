using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Bearded.TD.Content.Mods;

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
            packedSpriteSet);
    }

    private static ISpriteSetImplementation loadPackedSpriteSet(DirectoryInfo directory, ModLoadingContext modLoadingContext,
        Serialization.Models.SpriteSet jsonModel)
    {
        _ = jsonModel.Id ?? throw new InvalidDataException($"{nameof(jsonModel.Id)} must be non-null");

        var samplers = getSamplersWithPreAndSuffixes(jsonModel);
        var sprites = sortFilesBySpriteAndSampler(directory, samplers);

        return modLoadingContext.GraphicsLoader.CreateSpriteSet(
            samplers.Select(s => s.Sampler), sprites, jsonModel.Id);
    }

    private static IEnumerable<SpriteBitmaps>
        sortFilesBySpriteAndSampler(
            DirectoryInfo directory,
            List<(Sampler sampler, string prefix, string suffix)> samplers
        )
    {
        var sprites = new Dictionary<string, Dictionary<string, Lazy<Image<Bgra32>>>>();

        addPngFilesRecursively(directory, samplers, sprites, "");

        return sprites.Select(kvp => new SpriteBitmaps(kvp.Key, kvp.Value));
    }

    private static void addPngFilesRecursively(
        DirectoryInfo directory,
        List<(Sampler sampler, string prefix, string suffix)> samplers,
        Dictionary<string, Dictionary<string, Lazy<Image<Bgra32>>>> sprites,
        string spriteNamePrefix
    )
    {
        foreach (var file in pngFilesInFlat(directory))
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);

            var (sampler, prefix, suffix) = samplers
                .First(s => fileNameWithoutExtension.StartsWith(s.prefix) &&
                    fileNameWithoutExtension.EndsWith(s.suffix));

            var localSpriteName = fileNameWithoutExtension.Substring(
                prefix.Length,
                fileNameWithoutExtension.Length - prefix.Length - suffix.Length);

            var spriteName = spriteNamePrefix + localSpriteName;

            if (!sprites.ContainsKey(spriteName))
                sprites[spriteName] = new Dictionary<string, Lazy<Image<Bgra32>>>();

            sprites[spriteName][sampler.Name] = lazyReadBitmap(file);
        }

        foreach (var dir in directory.GetDirectories("*", SearchOption.TopDirectoryOnly))
        {
            var newPrefix = spriteNamePrefix + "." + dir.Name;

            addPngFilesRecursively(dir, samplers, sprites, newPrefix);
        }
    }

    private static Lazy<Image<Bgra32>> lazyReadBitmap(FileInfo file)
    {
        return new Lazy<Image<Bgra32>>(() =>
        {
            var fileStream = file.OpenRead();
            var image = Image.Load<Bgra32>(fileStream);
            fileStream.Close();
            return image;
        });
    }

    private static List<(Sampler Sampler, string Prefix, string Suffix)> getSamplersWithPreAndSuffixes(
        Serialization.Models.SpriteSet jsonModel)
    {
        var samplers = new List<(Sampler Sampler, string Prefix, string Suffix)>();

        if ((jsonModel.TextureSamplers?.Count ?? 0) == 0)
            throw new InvalidDataException("Sprites need to define at least one sampler in 'textureSamplers' array");

        foreach (var sampler in Enumerable.Reverse(jsonModel.TextureSamplers))
        {
            if (string.IsNullOrWhiteSpace(sampler.Sampler))
                throw new InvalidDataException("Texture sampler needs non empty 'sampler' property");

            samplers.Add(
                (new Sampler(sampler.Sampler, sampler.Transformations?.ToImmutableArray() ?? ImmutableArray<string>.Empty),
                    sampler.Prefix ?? "",
                    sampler.Suffix ?? ""));
        }

        return samplers;
    }

    private static IEnumerable<FileInfo> pngFilesInFlat(DirectoryInfo directory)
        => directory.GetFiles("*.png", SearchOption.TopDirectoryOnly);
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics.ImageSharp;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Textures;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities.IO;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using static System.Math;

namespace Bearded.TD.Rendering.Deferred.Level;

sealed class BiomeMaterials : IDisposable
{
    public readonly record struct Sampler(string name, ArrayTexture texture)
    {
        private string name { get; } = name;
        private ArrayTexture texture { get; } = texture;

        public ArrayTextureUniform GetUniform(TextureUnit unit) => new(name, unit, texture);
    }

    private readonly ImmutableDictionary<IBiome, byte> biomeIds;
    public ImmutableArray<Sampler> Samplers { get; }
    private readonly ImmutableArray<ArrayTexture> textures;

    public BiomeMaterials(GameInstance game)
    {
        biomeIds = setBiomeIds(game);
        (Samplers, textures) = createTextureSamplers(biomeIds, game.Meta.Logger);
    }

    public void Dispose()
    {
        foreach (var texture in textures)
            texture.Dispose();
    }

    public byte GetId(IBiome biome) => biomeIds[biome];

    private static (ImmutableArray<Sampler>, ImmutableArray<ArrayTexture>) createTextureSamplers(
        IDictionary<IBiome, byte> biomes, Logger logger)
    {
        var context = new SamplerBuildContext(biomes.OrderBy(t => t.Value).Select(t => t.Key).ToImmutableArray(), logger);
        var samplers = collectSamplers(context);
        return createArrayTextures(context, samplers);
    }

    private readonly record struct SamplerBuildContext(ImmutableArray<IBiome> Biomes, Logger Logger)
    {
        public int BiomeCount => Biomes.Length;
    }

    private static Dictionary<string, Dictionary<int, Image>> collectSamplers(SamplerBuildContext context)
    {
        Dictionary<string, Dictionary<int, Image>> samplers = new();

        for (var id = 0; id < context.BiomeCount; id++)
        {
            var biome = context.Biomes[id];
            foreach (var (sampler, image) in biome.Material.Textures)
            {
                if (!samplers.ContainsKey(sampler))
                    samplers.Add(sampler, new Dictionary<int, Image>());

                samplers[sampler].Add(id, image);
            }
        }

        logWarningIfSamplersIncomplete(samplers, context);

        return samplers;
    }

    private static (ImmutableArray<Sampler>, ImmutableArray<ArrayTexture>) createArrayTextures(
        SamplerBuildContext context, Dictionary<string, Dictionary<int, Image>> samplers)
    {
        var textureSamplers = ImmutableArray.CreateBuilder<Sampler>(samplers.Count);
        var textures = ImmutableArray.CreateBuilder<ArrayTexture>(samplers.Count);

        foreach (var (samplerName, imagesById) in samplers)
        {
            var arrayTexture = createArrayTexture(context, imagesById, samplerName);
            textureSamplers.Add(new Sampler(samplerName, arrayTexture));
            textures.Add(arrayTexture);
        }

        return (textureSamplers.MoveToImmutable(), textures.MoveToImmutable());
    }

    private static ArrayTexture createArrayTexture(
        SamplerBuildContext context, Dictionary<int, Image> imagesById, string samplerName)
    {
        var (w, h) = imagesById.Values.Aggregate(
            (W: 1, H: 1), (size, image) => (Max(size.W, image.Width), Max(size.H, image.Height)));

        var arrayTexture = ArrayTexture.Empty(w, h, context.BiomeCount);

        for (var id = 0; id < context.BiomeCount; id++)
        {
            if (!imagesById.TryGetValue(id, out var image))
                continue;

            var imageToUpload = ensureCorrectSize(image, w, h, context, id, samplerName);

            arrayTexture.PopulateLayerFrom(ImageTextureData.From(image), id);

            if(imageToUpload != image)
                imageToUpload.Dispose();
        }

        using var t = arrayTexture.Bind();
        t.GenerateMipmap();

        return arrayTexture;
    }

    private static Image ensureCorrectSize(
        Image image, int w, int h, SamplerBuildContext context, int biomeId, string samplerName)
    {
        var size = new Size(w, h);
        if (image.Size == size)
            return image;

        var biome = context.Biomes[biomeId];
        context.Logger.Warning?.Log(
            $"Mismatching texture size in sampler '{samplerName}' of material '{biome.Material.Id}' in biome '{biome.Id}': " +
            $"Expected {w}x{h}, found {image.Width}x{image.Height}");

        return image.Clone(c => c.Resize(size));
    }

    private static void logWarningIfSamplersIncomplete(
        Dictionary<string, Dictionary<int, Image>> samplers, SamplerBuildContext context)
    {
        var printedHeadline = false;

        foreach (var sampler in samplers)
        {
            var texCount = sampler.Value.Count;

            if (texCount == context.BiomeCount)
                continue;

            if (!printedHeadline)
            {
                context.Logger.Warning?.Log("Encountered incomplete samplers for biome materials:");
                printedHeadline = true;
            }

            var biome = context.Biomes[sampler.Value.Keys.First()];

            context.Logger.Warning?.Log(
                texCount == 1
                    ? $"- Sampler '{sampler.Key}' has only one material '{biome.Material.Id}' in biome '{biome.Id}'"
                    : $"- Sampler '{sampler.Key}' has {texCount}/{context.BiomeCount} textures"
            );
        }
    }

    private static ImmutableDictionary<IBiome, byte> setBiomeIds(GameInstance game)
    {
        return game.Blueprints.Biomes.All
            .Select((biome, id) => (biome, id))
            .ToImmutableDictionary(b => b.biome, b => (byte)b.id);
    }
}

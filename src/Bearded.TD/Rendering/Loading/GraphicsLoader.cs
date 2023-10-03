using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bearded.Graphics.ShaderManagement;
using Bearded.Graphics.Textures;
using Bearded.TD.Content;
using Bearded.TD.Content.Models;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Algorithms;
using Bearded.Utilities.IO;
using Bearded.Utilities.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static Bearded.TD.Rendering.Loading.SpriteTextureTransformations;

namespace Bearded.TD.Rendering.Loading;

sealed class GraphicsLoader : IGraphicsLoader
{
    readonly record struct Bitmaps(string Name, Dictionary<string, Image<Bgra32>> BitmapsBySampler);
    readonly record struct PositionedBitmaps(Bitmaps Bitmaps, int X, int Y);
    readonly record struct PackedBitmaps(IReadOnlyList<PositionedBitmaps> PositionedBitmaps, int Width, int Height);

    // TODO: use mod specific shader managers (tricky bit: hot reload)
    private readonly ShaderManager shaderManager;
    private readonly IActionQueue glActions;
    private readonly Logger logger;

    public GraphicsLoader(ShaderManager shaderManager, IActionQueue glActionQueue, Logger logger)
    {
        this.shaderManager = shaderManager;
        glActions = glActionQueue;
        this.logger = logger;
    }

    public ISpriteSetImplementation CreateSpriteSet(
        IEnumerable<Sampler> samplers, IEnumerable<SpriteBitmaps> sprites, string id, SpritePackMode packMode)
    {
        // Avoid multiple enumeration
        var samplerArray = samplers.ToImmutableArray();

        var spriteRectangles = sprites.Select(rectangleWithBitmaps).ToList();
        if (spriteRectangles.Count == 0)
        {
            logger.Warning?.Log($"Sprite set '{id}' contains no sprites.");
            return new PackedSpriteSetBuilder(new(), 0, 0).Build(glActions, new());
        }

        PackedBitmaps packedSprites = packMode switch
        {
            SpritePackMode.Any => binPack(spriteRectangles),
            SpritePackMode.Vertical => packVertically(spriteRectangles),
            SpritePackMode.Horizontal => packHorizontally(spriteRectangles),
            _ => throw new ArgumentOutOfRangeException(nameof(packMode), packMode, null)
        };

        PackedBitmaps binPack(List<BinPacking.Rectangle<Bitmaps>> rectangles)
        {
            var result = BinPacking.Pack(rectangles);

            return new PackedBitmaps(
                result.Rectangles.Select(b => new PositionedBitmaps(b.Value, b.X, b.Y)).ToList(),
                result.Width,
                result.Height
            );
        }

        PackedBitmaps packVertically(List<BinPacking.Rectangle<Bitmaps>> rectangles)
        {
            var height = rectangles.Sum(r => r.Height);
            var width = rectangles.Max(r => r.Width);
            var minWidth = rectangles.Min(r => r.Width);
            if (minWidth < width)
                logger.Warning?.Log($"Sprite set '{id}' contains sprites with different widths. " +
                                    $"This may cause issues with the sprite set.");

            var y = 0;

            return new PackedBitmaps(
                rectangles.Select(r =>
                {
                    var b = new PositionedBitmaps(r.Value, 0, y);
                    y += r.Height;
                    return b;
                }).ToList(),
                width,
                height
            );
        }

        PackedBitmaps packHorizontally(List<BinPacking.Rectangle<Bitmaps>> rectangles)
        {
            var width = rectangles.Sum(r => r.Width);
            var height = rectangles.Max(r => r.Height);
            var minHeight = rectangles.Min(r => r.Height);
            if (minHeight < height)
                logger.Warning?.Log($"Sprite set '{id}' contains sprites with different heights. " +
                                    $"This may cause issues with the sprite set.");
            var x = 0;

            return new PackedBitmaps(
                rectangles.Select(r =>
                {
                    var b = new PositionedBitmaps(r.Value, x, 0);
                    x += r.Width;
                    return b;
                }).ToList(),
                width,
                height
            );
        }

        var usedSamplers = packedSprites.PositionedBitmaps.SelectMany(r => r.Bitmaps.BitmapsBySampler.Keys).Distinct().ToList();
        var unusedSamplersCount = samplerArray.Select(s => s.Name).Except(usedSamplers).Count();

        if (unusedSamplersCount > 0)
            logger.Warning?.Log($"Sprite set specifies {unusedSamplersCount} samplers it does not use");

        var builder = new PackedSpriteSetBuilder(usedSamplers, packedSprites.Width, packedSprites.Height);

        foreach (var rectangle in packedSprites.PositionedBitmaps)
        {
            var sprite = rectangle.Bitmaps.Name;
            foreach (var (sampler, bitmap) in rectangle.Bitmaps.BitmapsBySampler)
            {
                builder.CopyBitmap(sprite, sampler, bitmap, rectangle.X, rectangle.Y);
                bitmap.Dispose();
            }
        }

        return builder.Build(glActions, transformationsBySampler(samplerArray));

        static Dictionary<string, IEnumerable<ITextureTransformation>> transformationsBySampler(
            IEnumerable<Sampler> samplers)
        {
            return samplers.ToDictionary(s => s.Name, s => s.Transformations.Select(
                t => t switch
                {
                    "normalFromHeight" => NormalFromHeight,
                    "test" => Test,
                    _ => throw new InvalidDataException($"Sprite sampler has unknown transformation '{t}'"),
                }
            ));
        }

        static BinPacking.Rectangle<Bitmaps>
            rectangleWithBitmaps(SpriteBitmaps bitmaps)
        {
            var bitmapsBySampler = bitmaps.BitmapsBySampler.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);

            return allImagesHaveSameSize(bitmapsBySampler.Values, out var size)
                ? rectangleForSpriteWithSize(bitmaps.SpriteId, bitmapsBySampler, size)
                : throw new InvalidDataException(
                    $"Sprite '{bitmaps.SpriteId}' has component images of different sizes");
        }

        static BinPacking.Rectangle<Bitmaps> rectangleForSpriteWithSize(
            string spriteId,
            Dictionary<string, Image<Bgra32>> bitmapsBySampler, (int width, int height) size)
        {
            return new(new(spriteId, bitmapsBySampler), size.width, size.height);
        }

        static bool allImagesHaveSameSize(ICollection<Image<Bgra32>> bitmaps, out (int width, int height) size)
        {
            var first = bitmaps.First();
            var (width, height) = (first.Width, first.Height);
            size = (width, height);

            return bitmaps.All(b => b.Width == width && b.Height == height);
        }
    }

    public IRendererShader CreateRendererShader(IList<ModShaderFile> shaders, string shaderProgramName)
    {
        var shadersToAdd = shaders.Where(s => !shaderManager.Contains(s.Type, s.FriendlyName)).ToList();

        if (shaderManager.TryGetRendererShader(shaderProgramName, out var shaderProgram))
        {
            if (shadersToAdd.Count == 0)
                return shaderProgram;

            throw new InvalidDataException($"Different shader program with name {shaderProgramName} already exists.");
        }

        return glActions.Run(glOperations).Result;

        IRendererShader glOperations()
        {
            shadersToAdd.Select(shaderFile).ForEach(shaderManager.Add);

            return shaderManager.RegisterRendererShader(b =>
            {
                foreach (var (type, _, name) in shaders)
                {
                    b.With(type, name);
                }
            }, shaderProgramName);
        }

        ShaderFile shaderFile(ModShaderFile data)
        {
            return new ShaderFile(data.Type, data.Filepath, data.FriendlyName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bearded.Graphics.Textures;
using Bearded.TD.Content;
using Bearded.TD.Content.Models;
using Bearded.Utilities.Algorithms;
using Bearded.Utilities.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Bearded.TD.Rendering.Loading;

sealed partial class GraphicsLoader
{
    public ISpriteSetImplementation CreateSpriteSet(
        IEnumerable<Sampler> samplers,
        IEnumerable<SpriteBitmaps> sprites,
        SpriteSetConfiguration config)
    {
        // Avoid multiple enumeration
        var samplerArray = samplers.ToImmutableArray();

        var spriteRectangles = sprites.Select(s => rectangleWithBitmapsPadded(s, config.Padding)).ToList();
        if (spriteRectangles.Count == 0)
        {
            logger.Warning?.Log($"Sprite set '{config.Id}' contains no sprites.");
            return new PackedSpriteSetBuilder([], 0, 0).Build(glActions, []);
        }

        var packedSprites = config.PackMode switch
        {
            SpritePackMode.Any => binPack(spriteRectangles),
            SpritePackMode.Vertical => packVertically(spriteRectangles, config.Id, logger),
            SpritePackMode.Horizontal => packHorizontally(spriteRectangles, config.Id, logger),
            _ => throw new ArgumentOutOfRangeException(nameof(config.PackMode), config.PackMode, null),
        };

        var usedSamplers = packedSprites.PositionedBitmaps
            .SelectMany(r => r.Bitmaps.BitmapsBySampler.Keys).Distinct().ToList();
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

        return builder.Build(glActions, transformationsBySampler(samplerArray, config.Premultiply));

    }

    private static PackedBitmaps binPack(List<BinPacking.Rectangle<Bitmaps>> rectangles)
    {
        var result = BinPacking.Pack(rectangles);

        return new PackedBitmaps(
            result.Rectangles.Select(b => new PositionedBitmaps(b.Value, b.X, b.Y)).ToList(),
            result.Width,
            result.Height
        );
    }

    private static PackedBitmaps packVertically(
        List<BinPacking.Rectangle<Bitmaps>> rectangles, string id, Logger logger)
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

    private static PackedBitmaps packHorizontally(
        List<BinPacking.Rectangle<Bitmaps>> rectangles, string id, Logger logger)
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


    private static Dictionary<string, IEnumerable<ITextureTransformation>> transformationsBySampler(
        IEnumerable<Sampler> samplers, bool prependPremultiply)
    {
        var premultiply = TextureTransformation.Premultiply;

        return samplers.ToDictionary(s => s.Name, s =>
        {
            var transformations = s.Transformations.Select(
                t => t switch
                {
                    "normalFromHeight" => SpriteTextureTransformations.NormalFromHeight,
                    "test" => SpriteTextureTransformations.Test,
                    _ => throw new InvalidDataException($"Sprite sampler has unknown transformation '{t}'"),
                }
            );
            return prependPremultiply ? transformations.Prepend(premultiply) : transformations;
        });
    }

    private static BinPacking.Rectangle<Bitmaps> rectangleWithBitmapsPadded(SpriteBitmaps bitmaps, int padding)
    {
        var bitmapsBySampler = bitmaps.BitmapsBySampler.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);

        return allImagesHaveSameSize(bitmapsBySampler.Values, out var size)
            ? rectangleForSpriteWithSize(bitmaps.SpriteId, bitmapsBySampler, pad(size, padding))
            : throw new InvalidDataException(
                $"Sprite '{bitmaps.SpriteId}' has component images of different sizes");
    }

    private static (int width, int height) pad((int width, int height) size, int padding)
    {
        return (size.width + padding, size.height + padding);
    }

    private static BinPacking.Rectangle<Bitmaps> rectangleForSpriteWithSize(
        string spriteId,
        Dictionary<string, Image<Bgra32>> bitmapsBySampler, (int width, int height) size)
    {
        return new(new(spriteId, bitmapsBySampler), size.width, size.height);
    }

    private static bool allImagesHaveSameSize(ICollection<Image<Bgra32>> bitmaps, out (int width, int height) size)
    {
        var first = bitmaps.First();
        var (width, height) = (first.Width, first.Height);
        size = (width, height);

        return bitmaps.All(b => b.Width == width && b.Height == height);
    }
}

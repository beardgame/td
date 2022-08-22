using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Bearded.Graphics.ShaderManagement;
using Bearded.Graphics.System.Drawing;
using Bearded.Graphics.Textures;
using Bearded.TD.Content;
using Bearded.TD.Content.Models;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Algorithms;
using Bearded.Utilities.IO;
using Bearded.Utilities.Threading;
using static Bearded.TD.Rendering.Loading.SpriteTextureTransformations;

namespace Bearded.TD.Rendering.Loading;

class GraphicsLoader : IGraphicsLoader
{
    private readonly RenderContext context;
    private readonly IActionQueue glActions;
    private readonly Logger logger;

    public GraphicsLoader(RenderContext context, IActionQueue glActionQueue, Logger logger)
    {
        this.context = context;
        glActions = glActionQueue;
        this.logger = logger;
    }

    public ISpriteSetImplementation CreateSpriteSet(IEnumerable<Sampler> samplers,
        IEnumerable<SpriteBitmaps> sprites, bool pixelate, string id)
    {
        var spriteRectangles = sprites.Select(rectangleWithBitmaps).ToList();
        if (spriteRectangles.Count == 0)
        {
            logger.Warning?.Log($"Sprite set '{id}' contains no sprites.");
            return new PackedSpriteSetBuilder(new(), 0, 0).Build(glActions, pixelate, new());
        }

        var packedSprites = BinPacking.Pack(spriteRectangles);

        var usedSamplers = packedSprites.Rectangles.SelectMany(r => r.Value.BitmapsBySampler.Keys).Distinct().ToList();
        var unusedSamplersCount = samplers.Select(s => s.Name).Except(usedSamplers).Count();

        if (unusedSamplersCount > 0)
            logger.Warning?.Log($"Sprite set specifies {unusedSamplersCount} samplers it does not use");

        var builder = new PackedSpriteSetBuilder(usedSamplers, packedSprites.Width, packedSprites.Height);

        foreach (var rectangle in packedSprites.Rectangles)
        {
            var sprite = rectangle.Value.Name;
            foreach (var (sampler, bitmap) in rectangle.Value.BitmapsBySampler)
            {
                builder.CopyBitmap(sprite, sampler, bitmap, rectangle.X, rectangle.Y);
                bitmap.Dispose();
            }
        }

        return builder.Build(glActions, pixelate, transformationsBySampler(samplers));

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

        static BinPacking.Rectangle<(string Name, Dictionary<string, Bitmap> BitmapsBySampler)>
            rectangleWithBitmaps(SpriteBitmaps bitmaps)
        {
            var bitmapsBySampler = bitmaps.BitmapsBySampler.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);

            return allImagesHaveSameSize(bitmapsBySampler.Values, out var size)
                ? rectangleForSpriteWithSize(bitmaps.SpriteId, bitmapsBySampler, size)
                : throw new InvalidDataException(
                    $"Sprite '{bitmaps.SpriteId}' has component images of different sizes");
        }

        static BinPacking.Rectangle<(string, Dictionary<string, Bitmap>)> rectangleForSpriteWithSize(
            string spriteId,
            Dictionary<string, Bitmap> bitmapsBySampler, (int width, int height) size)
        {
            return new((spriteId, bitmapsBySampler), size.width, size.height);
        }

        static bool allImagesHaveSameSize(ICollection<Bitmap> bitmaps, out (int width, int height) size)
        {
            var first = bitmaps.First();
            var (width, height) = (first.Width, first.Height);
            size = (width, height);

            return bitmaps.All(b => b.Width == width && b.Height == height);
        }
    }

    public IRendererShader CreateRendererShader(IList<ModShaderFile> shaders, string shaderProgramName)
    {
        // TODO: use mod specific shader managers (tricky bit: hot reload)
        var shaderManager = context.Shaders.ShaderManager;

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

    public ArrayTexture CreateArrayTexture(List<Bitmap> layers)
    {
        return glActions.Run(() =>
        {
            var textureData = ArrayTextureData
                .From(layers.Select(BitmapTextureData.From));
            var texture = ArrayTexture.From(textureData, t => t.GenerateMipmap());
            return texture;
        }).Result;
    }
}

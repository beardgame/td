using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using amulware.Graphics.ShaderManagement;
using amulware.Graphics.Textures;
using Bearded.TD.Content;
using Bearded.TD.Content.Models;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Algorithms;
using Bearded.Utilities.IO;
using Bearded.Utilities.Threading;
using OpenTK.Graphics.OpenGL;
using Shader = Bearded.TD.Content.Models.Shader;

namespace Bearded.TD.Rendering.Loading
{
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

        public ISpriteSetImplementation CreateSpriteSet(IEnumerable<string> samplers,
            IEnumerable<(string Sprite, Dictionary<string, Lazy<Bitmap>> BitmapsBySampler)> sprites,
            Shader shader, bool pixelate, string id)
        {
            var spriteRectangles = sprites.Select(rectangleWithBitmaps).ToList();
            if (spriteRectangles.Count == 0)
            {
                logger.Warning?.Log($"Sprite set '{id}' contains no sprites.");
                return new PackedSpriteSetBuilder(new List<string>(), 0, 0).Build(glActions, shader, pixelate);
            }

            var packedSprites = BinPacking.Pack(spriteRectangles);

            var usedSamplers = packedSprites.Rectangles.SelectMany(r => r.Value.BitmapsBySampler.Keys).Distinct().ToList();
            var unusedSamplers = samplers.Except(usedSamplers).ToList();

            if (unusedSamplers.Count > 0)
                // TODO: make this log line context aware
                // TODO: consider adding unused samplers as tiny/empty textures to prevent contamination
                logger.Warning?.Log($"Sprite set specifies {unusedSamplers.Count} samplers it does not use");

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

            return builder.Build(glActions, shader, pixelate);


            static BinPacking.Rectangle<(string Name, Dictionary<string, Bitmap> BitmapsBySampler)>
                rectangleWithBitmaps((string Sprite, Dictionary<string, Lazy<Bitmap>> BitmapsBySampler) sprite)
            {
                var bitmapsBySampler = sprite.BitmapsBySampler.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);

                return allImagesHaveSameSize(bitmapsBySampler.Values).Match(
                    size => rectangleForSpriteWithSize(sprite, bitmapsBySampler, size),
                    () => throw new InvalidDataException($"Sprite '{sprite.Sprite}' has component images of different sizes")
                );
            }

            static BinPacking.Rectangle<(string, Dictionary<string, Bitmap>)> rectangleForSpriteWithSize(
                (string Sprite, Dictionary<string, Lazy<Bitmap>> BitmapsBySampler) sprite,
                Dictionary<string, Bitmap> bitmapsBySampler, (int width, int height) size)
            {
                return new BinPacking.Rectangle<(string, Dictionary<string, Bitmap>)>(
                    (sprite.Sprite, bitmapsBySampler), size.width, size.height);
            }

            static Maybe<(int width, int height)> allImagesHaveSameSize(ICollection<Bitmap> bitmaps)
            {
                var first = bitmaps.First();
                var (width, height) = (first.Width, first.Height);

                return bitmaps.Skip(1).All(b => b.Width == width && b.Height == height)
                    ? Maybe.Just((width, height))
                    : Maybe.Nothing;
            }
        }

        public IRendererShader CreateRendererShader(IList<(ShaderType Type, string Filepath, string FriendlyName)> shaders, string shaderProgramName)
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

            return glActions.RunAndReturn(glOperations);

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

            ShaderFile shaderFile((ShaderType, string, string) data)
            {
                var (type, file, name) = data;

                return new ShaderFile(type, file, name);
            }
        }

        public ArrayTexture CreateArrayTexture(List<Bitmap> layers)
        {
            return glActions.RunAndReturn(() => ArrayTextureData.From(layers).ToTexture(t => t.GenerateMipmap()));
        }

    }
}

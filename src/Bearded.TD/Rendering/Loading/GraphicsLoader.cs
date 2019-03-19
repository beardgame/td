﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using amulware.Graphics;
using amulware.Graphics.ShaderManagement;
using Bearded.TD.Content;
using Bearded.TD.Content.Models;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Algorithms;
using Bearded.Utilities.Threading;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering.Loading
{
    class GraphicsLoader : IGraphicsLoader
    {
        private readonly RenderContext context;
        private readonly IActionQueue glActions;

        public GraphicsLoader(RenderContext context, IActionQueue glActionQueue)
        {
            this.context = context;
            glActions = glActionQueue;
        }

        public PackedSpriteSet CreateSpriteSet(IEnumerable<(Bitmap Image, string Name)> sprites)
        {
            var packedSprites = BinPacking.Pack(sprites.Select(rectangle));

            var builder = new PackedSpriteSetBuilder(packedSprites.Width, packedSprites.Height);

            foreach (var rectangle in packedSprites.Rectangles)
            {
                var bitmap = rectangle.Value;
                builder.CopyBitmap(bitmap, rectangle.X, rectangle.Y);
                bitmap.Image.Dispose();
            }

            return builder.Build(context, glActions);
        }
        
        private static BinPacking.Rectangle<(Bitmap Image, string Name)>
            rectangle((Bitmap Image, string Name) sprite)
        {
            return new BinPacking.Rectangle<(Bitmap, string)>(
                sprite, sprite.Image.Width, sprite.Image.Height);
        }

        public ISurfaceShader CreateShaderProgram(
            IList<(ShaderType Type, string Filepath, string FriendlyName)> shaders, string shaderProgramName)
        {
            return glActions.RunAndReturn(glOperations);

            ISurfaceShader glOperations()
            {
                shaders.Select(shaderFile).ForEach(context.Surfaces.Shaders.Add);

                return shaders.Aggregate(
                    context.Surfaces.Shaders.BuildShaderProgram(),
                    (builder, shader) => builder.With(shader.Type, shader.FriendlyName)
                ).As(shaderProgramName);
            }

            ShaderFile shaderFile((ShaderType, string, string) data)
            {
                var (type, file, name) = data;
                
                return new ShaderFile(type, file, name);
            }
        }

        public ArrayTexture CreateArrayTexture(List<Bitmap> layers)
        {
            return glActions.RunAndReturn(() => new ArrayTexture(layers));
        }

    }
}

using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
            var shaderManager = context.Surfaces.Shaders;

            var shadersToAdd = shaders.Where(s => !shaderManager.Contains(s.Type, s.FriendlyName)).ToList();
            var shaderProgram = shaderManager[shaderProgramName];

            if (shadersToAdd.Count == 0 && shaderProgram != null)
                return shaderProgram;
            
            return glActions.RunAndReturn(glOperations);

            ISurfaceShader glOperations()
            {
                shadersToAdd.Select(shaderFile).ForEach(shaderManager.Add);

                return shaderProgram ?? shaders.Aggregate(
                    shaderManager.BuildShaderProgram(),
                    (builder, shader) => builder.With(shader.Type, shader.FriendlyName)
                ).As(shaderProgramName);
            }

            ShaderFile shaderFile((ShaderType, string, string) data)
            {
                var (type, file, name) = data;

#if DEBUG
                file = adjustToReloadable(file);
#endif
                
                return new ShaderFile(type, file, name);
            }

            string adjustToReloadable(string file)
            {
                // point at shader files in the actual repo instead of the binary folder for easy live shader editing
                
                // C:\Users\amulware\git\td\
                // \bin\Bearded.TD\Debug\ -> \src\Bearded.TD\
                // assets\mods\default\gfx\shaders\default-terrain.vs

                var newFile = file
                    .Replace("\\", "/")
                    .Replace("/bin/Bearded.TD/Debug/", "/src/Bearded.TD/");

                if (File.Exists(newFile))
                    return newFile;
                
                return file;
            }
        }

        public ArrayTexture CreateArrayTexture(List<Bitmap> layers)
        {
            return glActions.RunAndReturn(() => new ArrayTexture(layers));
        }

    }
}

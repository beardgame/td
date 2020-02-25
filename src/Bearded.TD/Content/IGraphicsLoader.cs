using System.Collections.Generic;
using System.Drawing;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using OpenTK.Graphics.OpenGL;
using Shader = Bearded.TD.Content.Models.Shader;

namespace Bearded.TD.Content
{
    interface IGraphicsLoader
    {
        PackedSpriteSet CreateSpriteSet(IEnumerable<(Bitmap Image, string Name)> sprites, Shader shader,
            string defaultTextureSampler, bool pixelate);

        ISurfaceShader CreateShaderProgram(
            IList<(ShaderType Type, string Filepath, string FriendlyName)> shaders, string shaderProgramName);

        ArrayTexture CreateArrayTexture(List<Bitmap> layers);
    }
}

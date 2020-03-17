using System;
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
        PackedSpriteSet CreateSpriteSet(
            IEnumerable<string> samplers,
            IEnumerable<(string Sprite, Dictionary<string, Lazy<Bitmap>> BitmapsBySampler)> sprites,
            Shader shader,
            bool pixelate);

        ISurfaceShader CreateShaderProgram(
            IList<(ShaderType Type, string Filepath, string FriendlyName)> shaders, string shaderProgramName);

        ArrayTexture CreateArrayTexture(List<Bitmap> layers);
    }
}

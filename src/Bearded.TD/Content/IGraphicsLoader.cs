using System;
using System.Collections.Generic;
using System.Drawing;
using amulware.Graphics.ShaderManagement;
using amulware.Graphics.Textures;
using Bearded.TD.Content.Models;
using OpenToolkit.Graphics.OpenGL;
using Shader = Bearded.TD.Content.Models.Shader;

namespace Bearded.TD.Content
{
    interface IGraphicsLoader
    {
        PackedSpriteSet CreateSpriteSet(IEnumerable<string> samplers,
            IEnumerable<(string Sprite, Dictionary<string, Lazy<Bitmap>> BitmapsBySampler)> sprites,
            Shader shader,
            bool pixelate, string id);

        IRendererShader CreateRendererShader(
            IList<(ShaderType Type, string Filepath, string FriendlyName)> shaders, string shaderProgramName);

        ArrayTexture CreateArrayTexture(List<Bitmap> layers);
    }
}

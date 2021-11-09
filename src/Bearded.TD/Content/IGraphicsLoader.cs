using System;
using System.Collections.Generic;
using System.Drawing;
using Bearded.Graphics.ShaderManagement;
using Bearded.Graphics.Textures;
using Bearded.TD.Content.Models;
using OpenTK.Graphics.OpenGL;
using Shader = Bearded.TD.Content.Models.Shader;

namespace Bearded.TD.Content
{
    interface IGraphicsLoader
    {
        ISpriteSetImplementation CreateSpriteSet(IEnumerable<string> samplers,
            IEnumerable<(string Sprite, Dictionary<string, Lazy<Bitmap>> BitmapsBySampler)> sprites,
            bool pixelate, string id);

        IRendererShader CreateRendererShader(
            IList<(ShaderType Type, string Filepath, string FriendlyName)> shaders, string shaderProgramName);

        ArrayTexture CreateArrayTexture(List<Bitmap> layers);
    }
}

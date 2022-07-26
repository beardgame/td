using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using Bearded.Graphics.ShaderManagement;
using Bearded.Graphics.Textures;
using Bearded.TD.Content.Models;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Content;

readonly record struct ModShaderFile(ShaderType Type, string Filepath, string FriendlyName);
readonly record struct Sampler(string Name, ImmutableArray<string> Transformations);
readonly record struct SpriteBitmaps(string SpriteId, IDictionary<string, Lazy<Bitmap>> BitmapsBySampler);


interface IGraphicsLoader
{
    ISpriteSetImplementation CreateSpriteSet(
        IEnumerable<Sampler> samplers,
        IEnumerable<SpriteBitmaps> sprites,
        bool pixelate,
        string id);

    IRendererShader CreateRendererShader(IList<ModShaderFile> shaders, string shaderProgramName);

    ArrayTexture CreateArrayTexture(List<Bitmap> layers);
}

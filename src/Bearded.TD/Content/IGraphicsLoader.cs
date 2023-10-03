using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Graphics.ShaderManagement;
using Bearded.TD.Content.Models;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Bearded.TD.Content;

readonly record struct ModShaderFile(ShaderType Type, string Filepath, string FriendlyName);
readonly record struct Sampler(string Name, ImmutableArray<string> Transformations);
readonly record struct SpriteBitmaps(string SpriteId, IDictionary<string, Lazy<Image<Bgra32>>> BitmapsBySampler);

enum SpritePackMode
{
    Any = 0,
    Vertical = 1,
    Horizontal = 2,
}

interface IGraphicsLoader
{
    ISpriteSetImplementation CreateSpriteSet(
        IEnumerable<Sampler> samplers,
        IEnumerable<SpriteBitmaps> sprites,
        string id,
        SpritePackMode packMode);

    IRendererShader CreateRendererShader(IList<ModShaderFile> shaders, string shaderProgramName);
}

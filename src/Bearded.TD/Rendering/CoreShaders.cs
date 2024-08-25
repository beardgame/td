using System;
using Bearded.Graphics.ShaderManagement;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Rendering;

sealed class CoreShaders
{
    private readonly ShaderManager shaders = new();

    // TODO: use mod specific shader managers (tricky bit: hot reload) and remove this property
    public ShaderManager ShaderManager => shaders;

    public CoreShaders()
    {
        var shaderPath = Constants.Paths.Content.Asset("shaders/");

        shaders.AddRange(
            ShaderFileLoader.CreateDefault(shaderPath).Load(".")
        );
        new[]
        {
            "geometry", "uvcolor",
            "deferred/gSprite",
            "deferred/debug",
            "deferred/debugDepth",
            "deferred/compose",
            "deferred/copy",
            "deferred/copyDepth",
            "deferred/pointlight",
            "deferred/spotlight",
            "gi/cascade",
            "terrain-base",
            "biome-base",
            "intermediateLayerBlur",
        }.ForEach(name => shaders.RegisterRendererShaderFromAllShadersWithName(name));
    }

    public IRendererShader GetShaderProgram(string name)
    {
        return shaders.TryGetRendererShader(name, out var program)
            ? program
            : throw new ArgumentException($"Shader {name} not found.");
    }
}

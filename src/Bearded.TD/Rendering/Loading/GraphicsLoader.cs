using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.Graphics.ShaderManagement;
using Bearded.TD.Content;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.IO;
using Bearded.Utilities.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Bearded.TD.Rendering.Loading;

sealed partial class GraphicsLoader : IGraphicsLoader
{
    readonly record struct Bitmaps(string Name, Dictionary<string, Image<Bgra32>> BitmapsBySampler);
    readonly record struct PositionedBitmaps(Bitmaps Bitmaps, int X, int Y);
    readonly record struct PackedBitmaps(IReadOnlyList<PositionedBitmaps> PositionedBitmaps, int Width, int Height);

    // TODO: use mod specific shader managers (tricky bit: hot reload)
    private readonly ShaderManager shaderManager;
    private readonly IActionQueue glActions;
    private readonly Logger logger;

    public GraphicsLoader(ShaderManager shaderManager, IActionQueue glActionQueue, Logger logger)
    {
        this.shaderManager = shaderManager;
        glActions = glActionQueue;
        this.logger = logger;
    }

    public IRendererShader CreateRendererShader(IList<ModShaderFile> shaders, string shaderProgramName)
    {
        var shadersToAdd = shaders.Where(s => !shaderManager.Contains(s.Type, s.FriendlyName)).ToList();

        if (shaderManager.TryGetRendererShader(shaderProgramName, out var shaderProgram))
        {
            if (shadersToAdd.Count == 0)
                return shaderProgram;

            throw new InvalidDataException($"Different shader program with name {shaderProgramName} already exists.");
        }

        return glActions.Run(glOperations).Result;

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

        ShaderFile shaderFile(ModShaderFile data)
        {
            return new ShaderFile(data.Type, data.Filepath, data.FriendlyName);
        }
    }
}

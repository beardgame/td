using Bearded.Graphics.ShaderManagement;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;

namespace Bearded.TD.Content.Models;

sealed class Shader : IBlueprint
{
    public ModAwareId Id { get; }

    public IRendererShader RendererShader { get; }

    public Shader(ModAwareId id, IRendererShader rendererShader)
    {
        Id = id;
        RendererShader = rendererShader;
    }

}
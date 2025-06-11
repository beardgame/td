using System.Collections.Generic;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.ShaderManagement;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Rendering.Vertices;
using Bearded.Utilities.IO;
using SharpGLTF.Schema2;

namespace Bearded.TD.Tests.Content;

static class ModLoadingMocks
{
    public static ModLoadingContext CreateLoadingContext()
    {
        var logger = new Logger();
        var graphicsLoader = new MockGraphicsLoader();
        var profiler = new ModLoadingProfiler();
        return new ModLoadingContext(logger, graphicsLoader, profiler);
    }

    private sealed class MockGraphicsLoader : IGraphicsLoader
    {
        public ISpriteSetImplementation CreateSpriteSet(
            IEnumerable<Sampler> samplers,
            IEnumerable<SpriteBitmaps> sprites,
            SpriteSetConfiguration config) => new MockSpriteSetImplementation();

        public IMeshesImplementation CreateMeshes(ModelRoot modelRoot) => new MockMeshes();

        public IRendererShader CreateRendererShader(
            IList<ModShaderFile> shaders,
            string shaderProgramName) => new MockRendererShader();
    }

    private sealed class MockSpriteSetImplementation : ISpriteSetImplementation
    {
        public void Dispose() { }

        public SpriteParameters GetSpriteParameters(string name) => default;

        public DrawableSpriteSet<TVertex, TVertexData> MakeConcreteWith<TVertex, TVertexData>(
            SpriteSet spriteSet,
            IDrawableRenderers drawableRenderers,
            DrawOrderGroup drawGroup,
            int drawGroupOrderKey,
            CreateVertex<TVertex, TVertexData> createVertex,
            Shader shader)
            where TVertex : struct, IVertexData => default!;

        public (DrawableSpriteSet<TVertex, TVertexData>, IRenderer) MakeCustomRendererWith<TVertex, TVertexData>(
            IDrawableRenderers drawableRenderers,
            CreateVertex<TVertex, TVertexData> createVertex,
            Shader shader,
            params IRenderSetting[] customRenderSettings) where TVertex : struct, IVertexData => default;
    }

    private sealed class MockMeshes : IMeshesImplementation
    {
        public IMesh GetMesh(string key) => default!;

        public void Dispose() { }
    }

    private sealed class MockRendererShader : IRendererShader
    {
        public void UseOnRenderer(IRenderer renderer) {}
        public void RemoveFromRenderer(IRenderer renderer) {}
    }
}

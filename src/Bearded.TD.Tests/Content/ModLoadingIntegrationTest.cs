using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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
using FluentAssertions;
using SharpGLTF.Schema2;
using Xunit;

namespace Bearded.TD.Tests.Content
{
    public sealed class ModLoadingIntegrationTest
    {
        [Fact]
        public void AllModsLoadSuccessfully()
        {
            var allMods = new ModLister().GetAll().ToImmutableArray();
            var sortedMods = new ModSorter().SortByDependency(allMods);
            var logger = new Logger();
            var graphicsLoader = new MockGraphicsLoader();
            var profiler = new ModLoadingProfiler();
            var context = new ModLoadingContext(logger, graphicsLoader, profiler);

            var loadedMods = new List<Mod>();

            foreach (var modForLoading in sortedMods.Select(modMetadata => new ModForLoading(modMetadata)))
            {
                modForLoading.StartLoading(context, loadedMods.AsReadOnly());
                waitForModLoaded(modForLoading);
                if (!modForLoading.DidLoadSuccessfully)
                {
                    modForLoading.Rethrow();
                }
                loadedMods.Add(modForLoading.GetLoadedMod());
            }

            context.Errors.Should().BeEmpty("no errors should be thrown by any blueprints");
        }

        private static void waitForModLoaded(ModForLoading modForLoading)
        {
            while (!modForLoading.IsDone)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
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
}

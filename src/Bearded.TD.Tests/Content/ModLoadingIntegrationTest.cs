using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Threading;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.ShaderManagement;
using Bearded.Graphics.Textures;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Loading;
using Bearded.Utilities.IO;
using OpenTK.Graphics.OpenGL;
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
                IEnumerable<string> samplers,
                IEnumerable<(string Sprite, Dictionary<string, Lazy<Bitmap>> BitmapsBySampler)> sprites,
                Shader shader,
                bool pixelate,
                string id) => new MockSpriteSetImplementation();

            public IRendererShader CreateRendererShader(
                IList<(ShaderType Type, string Filepath, string FriendlyName)> shaders,
                string shaderProgramName) => new MockRendererShader();

            public ArrayTexture CreateArrayTexture(List<Bitmap> layers) => default!;
        }

        private sealed class MockSpriteSetImplementation : ISpriteSetImplementation
        {
            public void Dispose() { }

            public SpriteParameters GetSpriteParameters(string name) => default;

            public DrawableSpriteSet<TVertex, TVertexData> MakeConcreteWith<TVertex, TVertexData>(
                SpriteSet spriteSet,
                SpriteRenderers spriteRenderers,
                DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex)
                where TVertex : struct, IVertexData => default!;

            public (DrawableSpriteSet<TVertex, TVertexData>, IRenderer) MakeCustomRendererWith<TVertex, TVertexData>(
                SpriteRenderers spriteRenderers,
                DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex,
                params IRenderSetting[] customRenderSettings) where TVertex : struct, IVertexData => default;
        }

        private sealed class MockRendererShader : IRendererShader
        {
            public void UseOnRenderer(IRenderer renderer) {}
            public void RemoveFromRenderer(IRenderer renderer) {}
        }
    }
}

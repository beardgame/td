using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Rendering
{
    using Renderers = List<(int DrawOrderKey, IRenderer Renderer)>;

    sealed class SpriteRenderers
    {
        private sealed class DrawOrderKeyComparer : IComparer<(int DrawOrderKey, IRenderer Renderer)>
        {
            public int Compare((int DrawOrderKey, IRenderer Renderer) x, (int DrawOrderKey, IRenderer Renderer) y)
            {
                return x.DrawOrderKey.CompareTo(y.DrawOrderKey);
            }
        }

        private static readonly DrawOrderKeyComparer drawOrderKeyComparer = new();

        private readonly Dictionary<SpriteSet, Dictionary<(Type, Type, Shader, SpriteDrawGroup, int), object>> knownSpriteSets = new();
        private readonly Dictionary<SpriteDrawGroup, Renderers> renderersByDrawGroup =
            Enum.GetValues<SpriteDrawGroup>().ToDictionary(g => g, _ => new Renderers());

        private readonly IRenderSetting[] defaultRenderSettings;

        public SpriteRenderers(RenderContext context)
        {
            defaultRenderSettings = new IRenderSetting[]
            {
                context.Settings.ProjectionMatrix,
                context.Settings.ViewMatrix,
                context.Settings.FarPlaneDistance,
                context.Settings.ViewMatrixLevel
            };
        }

        public DrawableSpriteSet<TVertex, TVertexData> GetOrCreateDrawableSpriteSetFor<TVertex, TVertexData>(
            SpriteSet spriteSet, Shader shader, SpriteDrawGroup drawGroup, int drawGroupOrderKey, Func<DrawableSpriteSet<TVertex, TVertexData>> createDrawable)
            where TVertex : struct, IVertexData
        {
            var key = (typeof(TVertex), typeof(TVertexData), shader, drawGroup, drawGroupOrderKey);

            var spriteSets = knownSpriteSets.GetValueOrInsertNewDefaultFor(spriteSet);

            var drawable = (DrawableSpriteSet<TVertex, TVertexData>)
                spriteSets.GetOrInsert(key, createDrawableAndRegisterDefaultRenderer);

            return drawable;

            DrawableSpriteSet<TVertex, TVertexData> createDrawableAndRegisterDefaultRenderer()
            {
                var d = createDrawable();
                createAndRegisterRenderer(spriteSet, d, drawGroup, drawGroupOrderKey);
                return d;
            }
        }

        public IRenderer CreateCustomRendererFor<TVertex, TVertexData>(
                DrawableSpriteSet<TVertex, TVertexData> drawable,
                IRenderSetting[] customRenderSettings)
            where TVertex : struct, IVertexData
        {
            var renderer = drawable.CreateRendererWithSettings(
                customRenderSettings.Concat(defaultRenderSettings)
            );

            return renderer;
        }

        private void createAndRegisterRenderer<TVertex, TVertexData>(
            SpriteSet spriteSet, DrawableSpriteSet<TVertex, TVertexData> drawable,
            SpriteDrawGroup drawGroup, int drawGroupOrderKey)
            where TVertex : struct, IVertexData
        {
            var renderer = drawable.CreateRendererWithSettings(defaultRenderSettings);

            var renderers = renderersByDrawGroup[drawGroup];

            renderers.AddSorted((drawGroupOrderKey, renderer), drawOrderKeyComparer);
        }

        public void RenderDrawGroup(SpriteDrawGroup group)
        {
            foreach (var (_, renderer) in renderersByDrawGroup[group])
            {
                renderer.Render();
                // TODO: should we clear the related mesh builder here?
            }
        }

        public void Dispose()
        {
            foreach (var spriteSet in knownSpriteSets.Values)
            {
                foreach (var drawableSpriteSet in spriteSet.Values.Cast<IDisposable>())
                {
                    drawableSpriteSet.Dispose();
                }
            }

            foreach (var drawGroup in renderersByDrawGroup.Values)
            {
                foreach (var (_, renderer) in drawGroup)
                {
                    renderer.Dispose();
                }
            }
        }

        public void ClearAll()
        {
            foreach (var spriteSet in knownSpriteSets.Values)
            {
                foreach (var drawableSpriteSet in spriteSet.Values.Cast<IClearable>())
                {
                    drawableSpriteSet.Clear();
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Loading;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Rendering;

using Renderers = List<(int DrawOrderKey, IRenderer Renderer)>;

sealed class SpriteRenderers
{
    private sealed class DrawOrderKeyComparer : IComparer<(int DrawOrderKey, IRenderer)>
    {
        public int Compare((int DrawOrderKey, IRenderer) x, (int DrawOrderKey, IRenderer) y)
            => x.DrawOrderKey.CompareTo(y.DrawOrderKey);
    }

    // ReSharper disable NotAccessedPositionalProperty.Local
    private readonly record struct DrawGroup(SpriteDrawGroup Group, int OrderKey);
    private readonly record struct Key(
        Type Vertex, Type VertexData, SpriteSet SpriteSet, Shader Shader, DrawGroup DrawGroup);
    // ReSharper restore NotAccessedPositionalProperty.Local

    private static Key key<TVertex, TVertexData>(
        SpriteSet spriteSet, Shader shader, SpriteDrawGroup group, int orderKey)
        => new(typeof(TVertex), typeof(TVertexData), spriteSet, shader, new DrawGroup(group, orderKey));

    private static readonly DrawOrderKeyComparer drawOrderKeyComparer = new();

    private readonly Dictionary<Key, object> knownSpriteSets = new();
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
            context.Settings.CameraPosition,
            context.Settings.FarPlaneBaseCorner,
            context.Settings.FarPlaneUnitX,
            context.Settings.FarPlaneUnitY,
            context.Settings.Time,
        };
    }

    public DrawableSpriteSet<TVertex, TVertexData> GetOrCreateDrawableSpriteSetFor<TVertex, TVertexData>(
        SpriteSet spriteSet, Shader shader, SpriteDrawGroup drawGroup, int drawGroupOrderKey,
        Func<DrawableSpriteSet<TVertex, TVertexData>> createDrawable)
        where TVertex : struct, IVertexData
    {
        var key = key<TVertex, TVertexData>(spriteSet, shader, drawGroup, drawGroupOrderKey);

        if (knownSpriteSets.TryGetValue(key, out var obj))
            return (DrawableSpriteSet<TVertex, TVertexData>)obj;

        var newDrawable = createDrawable();
        createAndRegisterRenderer(newDrawable, drawGroup, drawGroupOrderKey);
        knownSpriteSets.Add(key, newDrawable);

        return newDrawable;
    }

    public IRenderer CreateCustomRendererFor<TVertex, TVertexData>(
        DrawableSpriteSet<TVertex, TVertexData> drawable,
        IRenderSetting[] customRenderSettings)
        where TVertex : struct, IVertexData
    {
        return drawable.CreateRendererWithSettings(
            customRenderSettings.Concat(defaultRenderSettings)
        );
    }

    private void createAndRegisterRenderer<TVertex, TVertexData>(
        DrawableSpriteSet<TVertex, TVertexData> drawable,
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
        foreach (var spriteSet in knownSpriteSets.Values.Cast<IDisposable>())
        {
            spriteSet.Dispose();
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
        foreach (var spriteSet in knownSpriteSets.Values.Cast<IClearable>())
        {
            spriteSet.Clear();
        }
    }
}

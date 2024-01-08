using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Loading;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Rendering;

using Renderers = List<(int DrawOrderKey, IRenderer Renderer)>;

interface IDrawable : IClearable, IDisposable
{
    IRenderer CreateRendererWithSettings(IEnumerable<IRenderSetting> settings);
}

// Right now this is just a marker interface to indicate intention
// It should perhaps contain the method creating the actual drawable, or be passed into it to avoid generics?
interface IDrawableTemplate;

sealed class SpriteRenderers : ISpriteRenderers
{
    private sealed class DrawOrderKeyComparer : IComparer<(int DrawOrderKey, IRenderer)>
    {
        public int Compare((int DrawOrderKey, IRenderer) x, (int DrawOrderKey, IRenderer) y)
            => x.DrawOrderKey.CompareTo(y.DrawOrderKey);
    }

    // ReSharper disable NotAccessedPositionalProperty.Local
    private readonly record struct DrawGroup(SpriteDrawGroup Group, int OrderKey);
    private readonly record struct Key(
        Type DrawableType, IDrawableTemplate Template, Shader Shader, DrawGroup DrawGroup);
    // ReSharper restore NotAccessedPositionalProperty.Local

    private static Key key<TDrawableType>(
        IDrawableTemplate template, Shader shader, SpriteDrawGroup group, int orderKey)
        => new(typeof(TDrawableType), template, shader, new DrawGroup(group, orderKey));

    private static readonly DrawOrderKeyComparer drawOrderKeyComparer = new();

    private readonly Dictionary<Key, object> knownDrawables = new();
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

    public TDrawableType GetOrCreateDrawableFor<TDrawableType>(
        IDrawableTemplate template, Shader shader, SpriteDrawGroup drawGroup, int drawGroupOrderKey,
        Func<TDrawableType> createDrawable)
        where TDrawableType : IDrawable
    {
        var key = key<TDrawableType>(template, shader, drawGroup, drawGroupOrderKey);

        if (knownDrawables.TryGetValue(key, out var obj))
            return (TDrawableType)obj;

        var newDrawable = createDrawable();
        createAndRegisterRenderer(newDrawable, drawGroup, drawGroupOrderKey);
        knownDrawables.Add(key, newDrawable);

        return newDrawable;
    }

    public IRenderer CreateCustomRendererFor(
        IDrawable drawable,
        IRenderSetting[] customRenderSettings)
    {
        return drawable.CreateRendererWithSettings(
            customRenderSettings.Concat(defaultRenderSettings)
        );
    }

    private void createAndRegisterRenderer(
        IDrawable drawable,
        SpriteDrawGroup drawGroup, int drawGroupOrderKey)
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
        foreach (var drawable in knownDrawables.Values.Cast<IDisposable>())
        {
            drawable.Dispose();
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
        foreach (var drawable in knownDrawables.Values.Cast<IClearable>())
        {
            drawable.Clear();
        }
    }
}

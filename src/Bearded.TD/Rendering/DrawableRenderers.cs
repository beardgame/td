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

sealed class DrawableRenderers : IDrawableRenderers
{
    private sealed class DrawOrderKeyComparer : IComparer<(int DrawOrderKey, IRenderer)>
    {
        public int Compare((int DrawOrderKey, IRenderer) x, (int DrawOrderKey, IRenderer) y)
            => x.DrawOrderKey.CompareTo(y.DrawOrderKey);
    }

    // ReSharper disable NotAccessedPositionalProperty.Local
    private readonly record struct DrawGroup(DrawOrderGroup Group, int OrderKey);
    private readonly record struct Key(
        Type DrawableType, IDrawableTemplate Template, Shader Shader, DrawGroup DrawGroup);
    // ReSharper restore NotAccessedPositionalProperty.Local

    private static Key key<TDrawableType>(
        IDrawableTemplate template, Shader shader, DrawOrderGroup group, int orderKey)
        => new(typeof(TDrawableType), template, shader, new DrawGroup(group, orderKey));

    private static readonly DrawOrderKeyComparer drawOrderKeyComparer = new();

    private readonly Dictionary<Key, object> knownDrawables = new();
    private readonly Dictionary<DrawOrderGroup, Renderers> renderersByDrawGroup =
        Enum.GetValues<DrawOrderGroup>().ToDictionary(g => g, _ => new Renderers());

    private readonly IRenderSetting[] defaultRenderSettings;

    public DrawableRenderers(CoreRenderSettings settings)
    {
        defaultRenderSettings =
        [
            settings.ProjectionMatrix,
            settings.ViewMatrix,
            settings.FarPlaneDistance,
            settings.CameraPosition,
            settings.FarPlaneBaseCorner,
            settings.FarPlaneUnitX,
            settings.FarPlaneUnitY,
            settings.Time,
            settings.UITime,
        ];
    }

    public TDrawableType GetOrCreateDrawableFor<TDrawableType>(
        IDrawableTemplate template, Shader shader, DrawOrderGroup drawGroup, int drawGroupOrderKey,
        Func<TDrawableType> createDrawable)
        where TDrawableType : IDrawable
    {
        var key = key<TDrawableType>(template, shader, drawGroup, drawGroupOrderKey);

        if (knownDrawables.TryGetValue(key, out var obj))
            return (TDrawableType)obj;

        var newDrawable = createDrawable();
        CreateAndRegisterRenderer(newDrawable, drawGroup, drawGroupOrderKey);
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

    public void CreateAndRegisterRenderer(
        IDrawable drawable,
        DrawOrderGroup group, int drawGroupOrderKey)
    {
        var renderer = drawable.CreateRendererWithSettings(defaultRenderSettings);

        RegisterRenderer(renderer, group, drawGroupOrderKey);
    }

    public void RegisterRenderer(IRenderer renderer, DrawOrderGroup group, int drawGroupOrderKey)
    {
        var renderers = renderersByDrawGroup[group];

        renderers.AddSorted((drawGroupOrderKey, renderer), drawOrderKeyComparer);
    }

    public void RenderDrawGroup(DrawOrderGroup group)
    {
        foreach (var (_, renderer) in renderersByDrawGroup[group])
        {
            renderer.Render();
            // TODO: should we clear the related mesh builder here?
        }
    }

    public void DisposeAll()
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

using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.ShaderManagement;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering;

namespace Bearded.TD.Testing.GameStates;

sealed class NoOpDrawableRenderers : IDrawableRenderers
{
    public TDrawableType GetOrCreateDrawableFor<TDrawableType>(
        IDrawableTemplate template, Shader shader, DrawOrderGroup drawGroup, int drawGroupOrderKey,
        Func<TDrawableType> createDrawable)
        where TDrawableType : IDrawable
    {
        throw new InvalidOperationException("Cannot create drawable sprites in tests");
    }

    public IRenderer CreateAndRegisterRendererFor(IRendererShader shader, DrawOrderGroup drawGroup,
        int drawGroupOrderKey,
        Func<IEnumerable<IRenderSetting>, IRenderer> createRenderer)
    {
        throw new InvalidOperationException("Cannot create renderers in tests");
    }

    public IRenderer CreateUnregisteredRendererFor(IDrawable drawable, IRenderSetting[] customRenderSettings)
    {
        throw new InvalidOperationException("Cannot create custom renderers in tests");
    }

    public IRenderer CreateAndRegisterRendererFor(IDrawable drawable, DrawOrderGroup group, int drawGroupOrderKey)
    {
        throw new InvalidOperationException("Cannot create renderers in tests");
    }

    public void RegisterRenderer(IRenderer renderer, DrawOrderGroup group, int drawGroupOrderKey)
    {
        throw new InvalidOperationException("Cannot register renderers in tests");
    }

    public void RenderDrawGroup(DrawOrderGroup @group) { }
    public void DisposeAll() { }
    public void ClearAll() { }
}

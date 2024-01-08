using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering;

namespace Bearded.TD.Testing.GameStates;

sealed class NoOpSpriteRenderers : ISpriteRenderers
{
    public TDrawableType GetOrCreateDrawableFor<TDrawableType>(
        IDrawableTemplate template, Shader shader, SpriteDrawGroup drawGroup, int drawGroupOrderKey,
        Func<TDrawableType> createDrawable)
        where TDrawableType : IDrawable
    {
        throw new InvalidOperationException("Cannot create drawable sprites in tests");
    }

    public IRenderer CreateCustomRendererFor(IDrawable drawable, IRenderSetting[] customRenderSettings)
    {
        throw new InvalidOperationException("Cannot create custom renderers in tests");
    }

    public void RenderDrawGroup(SpriteDrawGroup @group) { }
    public void Dispose() { }
    public void ClearAll() { }
}

using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Loading;

namespace Bearded.TD.Testing.GameStates;

sealed class NoOpSpriteRenderers : ISpriteRenderers
{
    public DrawableSpriteSet<TVertex, TVertexData> GetOrCreateDrawableSpriteSetFor<TVertex, TVertexData>(
        SpriteSet spriteSet, Shader shader, SpriteDrawGroup drawGroup, int drawGroupOrderKey, Func<DrawableSpriteSet<TVertex, TVertexData>> createDrawable) where TVertex : struct, IVertexData
    {
        throw new InvalidOperationException("Cannot create drawable sprites in tests");
    }

    public IRenderer CreateCustomRendererFor<TVertex, TVertexData>(
        DrawableSpriteSet<TVertex, TVertexData> drawable, IRenderSetting[] customRenderSettings) where TVertex : struct, IVertexData
    {
        throw new InvalidOperationException("Cannot create custom renderers in tests");
    }

    public void RenderDrawGroup(SpriteDrawGroup @group) { }
    public void Dispose() { }
    public void ClearAll() { }
}

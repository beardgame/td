using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics.Pipelines;
using Bearded.Graphics.Pipelines.Context;
using Bearded.Graphics.Textures;
using Bearded.TD.Content.Models;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using static Bearded.Graphics.Pipelines.Context.BlendMode;
using static Bearded.TD.Content.Models.DrawOrderGroup;

namespace Bearded.TD.Rendering;

using static Pipeline<LayerRenderer.State>;
using IPipeline = IPipeline<LayerRenderer.State>;

sealed class LayerRenderer
{
    private static readonly DrawOrderGroup[] uiDrawGroups =
    [
        UIBackground,
        UISpritesBack,
        UIFont,
        UISpritesTop,
    ];

    public readonly record struct State(IRenderLayer Layer, RenderTarget Target);

    private readonly DeferredRenderer deferredRenderer;
    private readonly IPipeline renderLayer;

    public LayerRenderer(CoreRenderSettings settings, CoreRenderers renderers, DeferredRenderer deferredRenderer)
    {
        this.deferredRenderer = deferredRenderer;

        renderLayer = WithContext(
            c => c.SetDebugName(s => $"Render layer {s.Layer}")
                .BindRenderTarget(s => s.Target)
                .SetScissorRegion(s => ScissorRegion.SingleOrFullTarget(s.Layer.RenderOptions.ClipDrawRegion)),
            InOrder(
                Do(s => settings.SetSettingsFor(s.Layer)),
                Do(tryRenderDeferred),
                Do(renderers.FlushGradients),
                WithContext(c => c.SetBlendMode(Premultiplied),
                    InOrder(
                        WithContext(c => c.SetDebugName("Render primitives"),
                            Render(
                                renderers.PrimitivesRenderer,
                                renderers.ConsoleBackgroundRenderer)
                        ),
                        WithContext(c => c.SetDebugName("Render UI"),
                            renderDrawGroups(renderers.DrawableRenderers, uiDrawGroups)
                        )
                    )),
                Do(renderers.ClearAll)
            )
        );
    }

    private static IPipeline renderDrawGroups(IDrawableRenderers renderers, IEnumerable<DrawOrderGroup> drawGroups)
    {
        return InOrder(drawGroups.Select(group =>
            WithContext(c => c.SetDebugName($"Group {group}"), Do(_ => renderers.RenderDrawGroup(group)))
        ));
    }

    private void tryRenderDeferred(State state)
    {
        if (state.Layer is not IDeferredRenderLayer deferredLayer)
            return;

        deferredRenderer.RenderLayer(deferredLayer, state.Target);
        deferredRenderer.ClearAll();
    }

    public void RenderLayer(IRenderLayer layer, RenderTarget renderTarget)
    {
        layer.Draw();
        renderLayer.Execute(new State(layer, renderTarget));
    }

    public void OnResize(ViewportSize viewPort)
    {
        deferredRenderer.OnResize(viewPort);
    }
}

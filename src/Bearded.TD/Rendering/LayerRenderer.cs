using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics.Pipelines;
using Bearded.Graphics.Pipelines.Context;
using Bearded.Graphics.Textures;
using Bearded.TD.Content.Models;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
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
        UIShapes,
        UISpritesBack,
        UIFont,
        UISpritesTop,
    ];

    public readonly record struct State(IRenderLayer Layer, RenderTarget Target, Vector2i viewport);

    private readonly DeferredRenderer deferredRenderer;
    private readonly IPipeline renderLayer;
    private ViewportSize viewport;

    public LayerRenderer(CoreRenderSettings settings, CoreRenderers renderers, DeferredRenderer deferredRenderer)
    {
        this.deferredRenderer = deferredRenderer;

        var blurIntermediateTexture = Pipeline.Texture(PixelInternalFormat.Rgba, label: "Blur intermediate texture");
        var blurIntermediateTarget = Pipeline.RenderTargetWithColors("Blur intermediate target", blurIntermediateTexture);

        renderLayer = WithContext(
            c => c.SetDebugName(s => $"Render layer {s.Layer.DebugName}")
                .SetScissorRegion(s => ScissorRegion.SingleOrFullTarget(s.Layer.RenderOptions.ClipDrawRegion)),
            InOrder(
                Do(s => settings.SetSettingsFor(s.Layer)),
                Resize(s => s.viewport, blurIntermediateTexture),
                WithContext(c => c
                        .SetDebugName("Intermediate blur")
                        .BindRenderTarget(blurIntermediateTarget),
                    Render(renderers.IntermediateLayerBlurRenderer)
                ),
                WithContext(c => c
                        .SetDebugName("Render layer")
                        .BindRenderTarget(s => s.Target),
                    InOrder(
                        Do(tryRenderDeferred),
                        Do(renderers.FlushShapes),
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
                            ))
                    )
                ),
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
        renderLayer.Execute(new State(layer, renderTarget, (viewport.Width, viewport.Height)));
    }

    public void OnResize(ViewportSize viewPort)
    {
        viewport = viewPort;
        deferredRenderer.OnResize(viewPort);
    }
}

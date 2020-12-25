using amulware.Graphics.Pipelines;
using amulware.Graphics.Pipelines.Context;
using amulware.Graphics.Textures;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using static amulware.Graphics.Pipelines.Context.BlendMode;

namespace Bearded.TD.Rendering
{
    using static Pipeline<LayerRenderer.State>;
    using IPipeline = IPipeline<LayerRenderer.State>;

    sealed class LayerRenderer
    {
        public readonly struct State
        {
            public IRenderLayer Layer { get; }
            public RenderTarget Target { get; }

            public State(IRenderLayer layer, RenderTarget target)
            {
                Layer = layer;
                Target = target;
            }
        }

        private readonly CoreRenderSettings settings;
        private readonly CoreRenderers renderers;
        private readonly DeferredRenderer deferredRenderer;
        private readonly IPipeline renderLayer;

        public LayerRenderer(CoreRenderSettings settings, CoreRenderers renderers, CoreShaders shaders)
        {
            this.settings = settings;
            this.renderers = renderers;
            deferredRenderer = new DeferredRenderer(settings, shaders);

            renderLayer = Do(prepareForRender)
                .Then(WithContext(
                    c => c.BindRenderTarget(s => s.Target)
                        .SetScissorRegion(s =>
                            ScissorRegion.SingleOrFullTarget(s.Layer.RenderOptions.ClipDrawRegion)),
                    InOrder(
                        Do(tryRenderDeferred),
                        WithContext(c => c.SetBlendMode(Premultiplied),
                            Render(
                                renderers.PrimitivesRenderer,
                                renderers.ConsoleBackgroundRenderer,
                                renderers.UIFontRenderer,
                                renderers.ConsoleFontRenderer)
                        )
                    )
                ));
        }

        private void prepareForRender(State state)
        {
            settings.SetSettingsFor(state.Layer);
            renderers.ClearAll();
            deferredRenderer.ClearAll();
            state.Layer.Draw();
        }

        private void tryRenderDeferred(State state)
        {
            if (state.Layer is IDeferredRenderLayer deferredLayer)
            {
                deferredRenderer.RenderLayer(deferredLayer, state.Target);

                // TODO: not implemented yet
                //if (UserSettings.Instance.Debug.Deferred)
                //    deferredRenderer.RenderDebug();
            }
        }

        public void RenderLayer(IRenderLayer layer, RenderTarget renderTarget)
        {
            renderLayer.Execute(new State(layer, renderTarget));
        }

        public void OnResize(ViewportSize viewPort)
        {
            deferredRenderer.OnResize(viewPort);
        }
    }
}

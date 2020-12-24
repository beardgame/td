using amulware.Graphics.Pipelines;
using amulware.Graphics.Pipelines.Context;
using amulware.Graphics.Textures;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using OpenTK.Mathematics;
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

        private readonly SurfaceManager surfaces;
        private readonly DeferredRenderer deferredRenderer;
        private readonly IPipeline renderLayer;

        public LayerRenderer(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;
            deferredRenderer = new DeferredRenderer(surfaces);

            renderLayer = WithContext(
                c => c.BindRenderTarget(s => s.Target)
                    .SetScissorRegion(s => ScissorRegion.SingleOrFullTarget(s.Layer.RenderOptions.ClipDrawRegion)),
                InOrder(
                    Do(tryRenderDeferred),
                    WithContext(c => c.SetBlendMode(Premultiplied),
                        Render(
                            surfaces.PrimitivesRenderer,
                            surfaces.ConsoleBackgroundRenderer,
                            surfaces.UIFontRenderer,
                            surfaces.ConsoleFontRenderer)
                    )
                )
            );
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
            surfaces.ClearAll();
            layer.Draw();
            setUniformsFrom(layer);
            renderLayer.Execute(new State(layer, renderTarget));
        }

        private void setUniformsFrom(IRenderLayer layer)
        {
            surfaces.ViewMatrix.Value = layer.ViewMatrix;
            surfaces.ProjectionMatrix.Value = layer.ProjectionMatrix;

            if (layer is IDeferredRenderLayer deferredLayer)
                setUniformsFrom(deferredLayer);
        }

        private void setUniformsFrom(IDeferredRenderLayer deferredRenderLayer)
        {
            surfaces.FarPlaneDistance.Value = deferredRenderLayer.FarPlaneDistance;
            surfaces.Time.Value = deferredRenderLayer.Time;

            setFarPlaneParameters(deferredRenderLayer);
        }

        private void setFarPlaneParameters(IRenderLayer renderLayer)
        {
            var projection = renderLayer.ProjectionMatrix;
            var view = renderLayer.ViewMatrix;

            var cameraTranslation = view.ExtractTranslation();
            var viewWithoutTranslation = view.ClearTranslation();

            var projectionView = viewWithoutTranslation * projection;
            var projectionViewInverted = projectionView.Inverted();

            // check if multiplication by the inverted view matrix is working correctly
            var baseCorner = new Vector4(-1, -1, 1, 1) * projectionViewInverted;
            baseCorner = baseCorner / baseCorner.W;

            var xCorner = new Vector4(1, -1, 1, 1) * projectionViewInverted;
            xCorner = xCorner / xCorner.W;

            var yCorner = new Vector4(-1, 1, 1, 1) * projectionViewInverted;
            yCorner = yCorner / yCorner.W;

            surfaces.FarPlaneBaseCorner.Value = baseCorner.Xyz;
            surfaces.FarPlaneUnitX.Value = xCorner.Xyz - baseCorner.Xyz;
            surfaces.FarPlaneUnitY.Value = yCorner.Xyz - baseCorner.Xyz;
            surfaces.CameraPosition.Value = cameraTranslation;
        }

        public void OnResize(ViewportSize viewPort)
        {
            deferredRenderer.OnResize(viewPort);
        }
    }
}

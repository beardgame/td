using amulware.Graphics.Pipelines;
using amulware.Graphics.Textures;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using OpenToolkit.Graphics.OpenGL;
using OpenToolkit.Mathematics;
using static amulware.Graphics.Pipelines.Context.BlendMode;

namespace Bearded.TD.Rendering
{
    using static Pipeline<RenderTarget>;

    sealed class LayerRenderer
    {
        private readonly SurfaceManager surfaces;
        private readonly DeferredRenderer deferredRenderer;
        private readonly IPipeline<RenderTarget> renderSurfaces;

        public LayerRenderer(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;
            deferredRenderer = new DeferredRenderer(surfaces);

            renderSurfaces = WithContext(
                    c => c.BindRenderTarget(t => t)
                        .SetBlendMode(Premultiplied),
                    Render(
                        surfaces.PrimitivesRenderer,
                        surfaces.UIFontRenderer,
                        surfaces.ConsoleBackgroundRenderer,
                        surfaces.ConsoleFontRenderer
                        )
                );
        }

        public void RenderLayer(IRenderLayer layer, RenderTarget renderTarget)
        {
            surfaces.ClearAll();
            layer.Draw();
            setUniformsFrom(layer);
            setClipRegionFrom(layer.RenderOptions);
            render(layer, renderTarget);
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

        private void setClipRegionFrom(RenderOptions options)
        {
            if (options.ClipDrawRegion.HasValue)
            {
                var ((x, y), (w, h)) = options.ClipDrawRegion.Value;
                GL.Enable(EnableCap.ScissorTest);
                GL.Scissor(x, y, w, h);
            }
            else
            {
                GL.Disable(EnableCap.ScissorTest);
            }
        }

        private void render(IRenderLayer layer, RenderTarget renderTarget)
        {
            if (layer is IDeferredRenderLayer deferredLayer)
            {
                deferredRenderer.RenderLayer(deferredLayer, renderTarget);

                // TODO: not implemented yet
                //if (UserSettings.Instance.Debug.Deferred)
                //    deferredRenderer.RenderDebug();
            }

            renderSurfaces.Execute(renderTarget);
        }

        public void OnResize(ViewportSize viewPort)
        {
            deferredRenderer.OnResize(viewPort);
        }
    }
}

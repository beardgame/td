using System;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Layers
{
    abstract class RenderLayerCompositeControl : CompositeControl, IRenderLayer
    {
        private IRendererRouter renderRouter = null!;
        private bool skipNextRender;

        public override void Render(IRendererRouter router)
        {
            if (skipNextRender)
            {
                skipNextRender = false;
                return;
            }

            RenderAsLayer(router);
        }

        public void RenderAsLayer(IRendererRouter router)
        {
            renderRouter = router;
            renderDescendantRenderLayerControlsBefore(this);
            router.Render(this);
            renderDescendantRenderLayerControlsAfter(this);
            renderRouter = null;
        }

        private void renderDescendantRenderLayerControlsBefore(IControlParent parent)
        {
            callOnAllVisibleDescendantLayers(parent, layer => layer.RenderAsLayerBeforeAncestorLayer);
        }
        private void renderDescendantRenderLayerControlsAfter(IControlParent parent)
        {
            callOnAllVisibleDescendantLayers(parent, layer => layer.RenderAsLayerAfterAncestorLayer);
        }

        private void callOnAllVisibleDescendantLayers(IControlParent parent, Func<RenderLayerCompositeControl, Action<IRendererRouter>> getRenderFunction)
        {
            foreach (var control in parent.Children)
            {
                if(!control.IsVisible)
                    continue;

                switch (control)
                {
                    case RenderLayerCompositeControl renderLayer:
                        getRenderFunction(renderLayer)(renderRouter);
                        break;
                    case IControlParent controlParent:
                        callOnAllVisibleDescendantLayers(controlParent, getRenderFunction);
                        break;
                }
            }
        }

        protected virtual void RenderAsLayerBeforeAncestorLayer(IRendererRouter router)
        {
        }

        protected virtual void RenderAsLayerAfterAncestorLayer(IRendererRouter router)
        {
        }

        public void SkipNextRender()
        {
            skipNextRender = true;
        }

        public abstract Matrix4 ViewMatrix { get; }
        public abstract Matrix4 ProjectionMatrix { get; }
        public abstract RenderOptions RenderOptions { get; }

        protected ViewportSize ViewportSize { get; private set; }

        public virtual void UpdateViewport(ViewportSize viewport)
        {
            ViewportSize = viewport;
        }

        public virtual void Draw()
        {
            base.Render(renderRouter);
        }
    }
}

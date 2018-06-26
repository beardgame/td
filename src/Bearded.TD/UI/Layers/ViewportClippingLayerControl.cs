
namespace Bearded.TD.UI.Layers
{
    class ViewportClippingLayerControl : DefaultRenderLayerControl
    {
        public override RenderOptions RenderOptions => new RenderOptions(false, getViewportFromFrame());
        
        private ((int, int), (int, int)) getViewportFromFrame()
        {
            var frame = Frame;
            var topLeft = frame.TopLeft;
            var size = frame.Size;

            return (((int)topLeft.X, (int)topLeft.Y), ((int)size.X, (int)size.Y));
        }
    }
}
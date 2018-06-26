
namespace Bearded.TD.UI.Layers
{
    struct RenderOptions
    {
        public bool RenderDeferred { get; }
        public ((int X, int Y), (int W, int H))? OverrideViewport { get; }
        
        public RenderOptions(bool renderDeferred, ((int x, int y), (int w, int h))? overrideViewport = null)
        {
            RenderDeferred = renderDeferred;
            OverrideViewport = overrideViewport;
        }
        
        public static RenderOptions Default => new RenderOptions();
        public static RenderOptions Game => new RenderOptions(true);
     }
}


namespace Bearded.TD.UI.Layers
{
    struct RenderOptions
    {
        public ((int X, int Y), (int W, int H))? OverrideViewport { get; }
        
        public RenderOptions(((int x, int y), (int w, int h))? overrideViewport = null)
        {
            OverrideViewport = overrideViewport;
        }
        
        public static RenderOptions Default => new RenderOptions();
     }
}


namespace Bearded.TD.UI.Layers
{
    struct RenderOptions
    {
        public ((int X, int Y), (int W, int H))? ClipDrawRegion { get; }

        public RenderOptions(((int x, int y), (int w, int h))? clipDrawRegion = null)
        {
            ClipDrawRegion = clipDrawRegion;
        }

        public static RenderOptions Default => new RenderOptions();
     }
}

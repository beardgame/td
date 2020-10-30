
using System.Drawing;

namespace Bearded.TD.UI.Layers
{
    struct RenderOptions
    {
        public Rectangle? ClipDrawRegion { get; }

        public RenderOptions(Rectangle? clipDrawRegion = null)
        {
            ClipDrawRegion = clipDrawRegion;
        }

        public static RenderOptions Default => new RenderOptions();
     }
}

using System.Collections.Generic;
using System.Drawing;
using Bearded.TD.Content.Models;

namespace Bearded.TD.Content
{
    interface IGraphicsLoader
    {
        PackedSpriteSet CreateSpriteSet(IEnumerable<(Bitmap Image, string Name)> sprites);
    }
}

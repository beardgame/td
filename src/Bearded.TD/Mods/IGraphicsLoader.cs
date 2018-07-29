using System.Collections.Generic;
using System.Drawing;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Mods
{
    interface IGraphicsLoader
    {
        PackedSpriteSet CreateSpriteSet(IEnumerable<(Bitmap Image, string Name)> sprites);
    }
}

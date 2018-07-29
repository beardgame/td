using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Bearded.TD.Mods;
using Bearded.TD.Mods.Models;
using Bearded.Utilities.Algorithms;
using Bearded.Utilities.Threading;

namespace Bearded.TD.Rendering.Loading
{
    partial class GraphicsLoader : IGraphicsLoader
    {
        private readonly RenderContext context;
        private readonly IActionQueue glActions;

        public GraphicsLoader(RenderContext context, IActionQueue glActionQueue)
        {
            this.context = context;
            glActions = glActionQueue;
        }

        public PackedSpriteSet CreateSpriteSet(IEnumerable<(Bitmap Image, string Name)> sprites)
        {
            var packedSprites = BinPacking.Pack(sprites.Select(rectangle));

            var builder = new PackedSpriteSetBuilder(packedSprites.Width, packedSprites.Height);

            foreach (var rectangle in packedSprites.Rectangles)
            {
                builder.CopyBitmap(rectangle.Value, rectangle.X, rectangle.Y);
                rectangle.Value.Image.Dispose();
            }

            return builder.Build(context, glActions);
        }

        private static BinPacking.Rectangle<(Bitmap Image, string Name)>
            rectangle((Bitmap Image, string Name) sprite)
        {
            return new BinPacking.Rectangle<(Bitmap, string)>(
                sprite, sprite.Image.Width, sprite.Image.Height);
        }
    }
}

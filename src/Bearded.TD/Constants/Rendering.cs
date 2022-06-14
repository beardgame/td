
using System;

namespace Bearded.TD;

static partial class Constants
{
    public static class Rendering
    {
        private const float pixelsPerTileSpriteResolution = 44;

        [Obsolete("we don't tie things to pixels anymore - perhaps we'll want some sort of concept of 'linewidth' instead though? (e.g. outlines, and for some effects like lasers?)")]
        public const float PixelSize = Game.World.HexagonWidth / pixelsPerTileSpriteResolution;

        public const float WallHeight = 1;
    }
}

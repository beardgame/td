namespace Bearded.TD
{
    static partial class Constants
    {
        public static class Rendering
        {
            private const float pixelsPerTileSpriteResolution = 44;

            public const float PixelsPerTileLevelResolution = pixelsPerTileSpriteResolution / 2;
            public const float PixelsPerTileCompositeResolution = 10000;

            public const float PixelSize = Game.World.HexagonWidth / pixelsPerTileSpriteResolution;

        }
    }
}

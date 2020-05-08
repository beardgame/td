using Bearded.TD.Meta;

namespace Bearded.TD
{
    static partial class Constants
    {
        public static class Rendering
        {
            private const float pixelsPerTileSpriteResolution = 44;

            public static float PixelsPerTileLevelResolution => pixelsPerTileSpriteResolution * UserSettings.Instance.Debug.TerrainDetail;
            public const float PixelsPerTileCompositeResolution = 10000;

            public const float PixelSize = Game.World.HexagonWidth / pixelsPerTileSpriteResolution;

        }
    }
}

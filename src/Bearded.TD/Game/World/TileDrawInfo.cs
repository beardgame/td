using System;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.World
{
    struct TileDrawInfo
    {
        public Unit Height { get; }
        public float HexScale { get; }

        private TileDrawInfo(float height, float hexScale)
            : this(height.U(), hexScale)
        {
        }

        public TileDrawInfo(Unit height, float hexScale)
        {
            Height = height;
            HexScale = hexScale;
        }

        public static TileDrawInfo For(TileGeometry geometry)
        {
            var (height, hexScale) = defaultParametersFor(geometry);

            return new TileDrawInfo(height, hexScale);
        }

        private static (float height, float hexScale) defaultParametersFor(TileGeometry geometry)
        {
            switch (geometry.Type)
            {
                case TileType.Unknown:
                    return (0, 0);
                case TileType.Floor:
                    return (rnd(0, 0.05f), rnd(0.9f, 0.9f));
                case TileType.Wall:
                    return (.4f + .4f * (float) geometry.Hardness, rnd(0.3f, 0.7f));
//                    return (rnd(0.4f, 0.8f), rnd(0.3f, 0.7f));
                case TileType.Crevice:
                    return (-3, rnd(0.1f, 0.5f));
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometry.Type), geometry.Type, null);
            }
        }

        private static float rnd(float min, float max) => StaticRandom.Float(min, max);
    }
}

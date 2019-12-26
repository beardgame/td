using System;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.World
{
    struct TileDrawInfo
    {
        public Unit Height { get; }
        public float HexScale { get; }

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

        private static (Unit height, float hexScale) defaultParametersFor(TileGeometry geometry)
        {
            switch (geometry.Type)
            {
                case TileType.Unknown:
                    return (Unit.Zero, 0);
                case TileType.Floor:
                    return (geometry.FloorHeight, rnd(0.75f, 0.9f));
                case TileType.Wall:
                    return (geometry.FloorHeight + 1.U(), rnd(0.3f, 0.7f));
                case TileType.Crevice:
                    return (geometry.FloorHeight - 3.U(), rnd(0.1f, 0.5f));
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometry.Type), geometry.Type, null);
            }
        }

        private static float rnd(float min, float max) => StaticRandom.Float(min, max);
    }
}

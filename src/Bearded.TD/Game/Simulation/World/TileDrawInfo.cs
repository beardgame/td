using System;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World
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
                    return (geometry.FloorHeight, 0.8f);
                case TileType.Wall:
                    return (geometry.FloorHeight + 1.U(), 0.2f);
                case TileType.Crevice:
                    return (geometry.FloorHeight - 3.U(), 1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometry.Type), geometry.Type, null);
            }
        }
    }
}
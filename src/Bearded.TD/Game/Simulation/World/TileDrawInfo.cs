using System;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World;

readonly struct TileDrawInfo
{
    public Unit Height { get; }
    public float HexScale { get; }

    public TileDrawInfo(Unit height, float hexScale)
    {
        Height = height;
        HexScale = hexScale;
    }

    public static Tilemap<TileDrawInfo> DrawInfosFromTypes(Tilemap<TileGeometry> tileGeometries)
    {
        return new(tileGeometries.Radius, t => fromType(tileGeometries[t]));
    }

    private static TileDrawInfo fromType(TileGeometry geometry)
    {
        var (height, hexScale) = geometry.Type switch
        {
            TileType.Unknown => (Unit.Zero, 0),
            TileType.Floor => (geometry.FloorHeight, 0.8f),
            TileType.Wall => (geometry.FloorHeight + 1.U(), 0.2f),
            TileType.Crevice => (geometry.FloorHeight - 3.U(), 1),
            _ => throw new NotSupportedException($"Tile type {nameof(geometry.Type)} is not supported.")
        };

        return new TileDrawInfo(height, hexScale);
    }
}
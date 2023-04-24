using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Input;

sealed class TileSelection
{
    public static TileSelection FromFootprint(IFootprint footprint) => new(footprint);

    public static TileSelection Single { get; } = new(Footprint.Single);

    private readonly IFootprint footprint;

    private TileSelection(IFootprint footprint)
    {
        this.footprint = footprint;
    }

    public PositionedFootprint GetPositionedFootprint(Position2 position)
    {
        var tile = footprint.RootTileClosestToWorldPosition(position);
        var orientation = findOrientation(position, tile);

        return new PositionedFootprint(footprint, tile, orientation);
    }

    private static Orientation findOrientation(Position2 position, Tile tile)
    {
        var offset = position - Level.GetPosition(tile);
        var direction = offset.Direction;
        return direction.HexagonalOrientation();
    }
}

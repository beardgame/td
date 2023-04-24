using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Input;

sealed class TileSelection
{
    public static TileSelection FromFootprint(IFootprint footprint) => new TileSelection(footprint);

    public static TileSelection Single { get; } = new TileSelection(Footprint.Single);

    private readonly IFootprint footprint;

    private TileSelection(IFootprint footprint)
    {
        this.footprint = footprint;
    }

    public PositionedFootprint GetPositionedFootprint(Position2 position)
    {
        return new PositionedFootprint(footprint, footprint.RootTileClosestToWorldPosition(position));
    }
}

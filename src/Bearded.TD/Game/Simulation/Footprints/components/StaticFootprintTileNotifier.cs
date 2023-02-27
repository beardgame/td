using System.Collections.Generic;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Footprints;

sealed class StaticFootprintTileNotifier : TileNotifierBase
{
    private readonly PositionedFootprint footprint;

    protected override IEnumerable<Tile> OccupiedTiles => footprint.OccupiedTiles;

    public StaticFootprintTileNotifier(PositionedFootprint footprint)
    {
        this.footprint = footprint;
    }

    protected override void OnAdded()
    {
        base.OnAdded();
        Events.Send(new FootprintChanged(footprint));
    }

    public override void Update(TimeSpan elapsedTime) {}
}

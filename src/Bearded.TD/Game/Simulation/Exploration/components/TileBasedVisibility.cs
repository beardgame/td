using System.Linq;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Exploration;

[Component("tileBasedVisibility")]
sealed class TileBasedVisibility : Component, IVisibility
{
    private ITilePresence? tilePresence;

    public ObjectVisibility Visibility => (tilePresence?.OccupiedTiles ?? Enumerable.Empty<Tile>())
        .Any(t => Owner.Game.VisibilityLayer[t].IsVisible())
            ? ObjectVisibility.Visible
            : ObjectVisibility.Invisible;

    protected override void OnAdded() {}

    public override void Activate()
    {
        base.Activate();
        tilePresence = Owner.GetTilePresence();
    }

    public override void Update(TimeSpan elapsedTime) {}
}

using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Exploration;

sealed class TileBasedVisibility<T> : Component<T>, IVisibility
    where T : GameObject, IComponentOwner<T>
{
    private readonly OccupiedTilesTracker occupiedTilesTracker = new();

    public ObjectVisibility Visibility =>
        occupiedTilesTracker.OccupiedTiles.Any(t => Owner.Game.VisibilityLayer[t].IsVisible())
            ? ObjectVisibility.Visible
            : ObjectVisibility.Invisible;

    protected override void OnAdded()
    {
        occupiedTilesTracker.Initialize(Owner, Events);
    }

    public override void OnRemoved()
    {
        occupiedTilesTracker.Dispose(Events);
        base.OnRemoved();
    }

    public override void Update(TimeSpan elapsedTime) {}
}

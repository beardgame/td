using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Exploration;

[Component("revealSurroundingZones")]
sealed class RevealSurroundingZones<T> : Component<T, IRevealSurroundingZonesParameters>
    where T : IGameObject, IComponentOwner<T>
{
    private readonly OccupiedTilesTracker occupiedTilesTracker = new();

    public RevealSurroundingZones(IRevealSurroundingZonesParameters parameters) : base(parameters)
    {
        occupiedTilesTracker.TileAdded += onTileAdded;
    }

    private void onTileAdded(Tile t)
    {
        if (Owner.Game.ZoneLayer.ZoneForTile(t) is { } zone)
        {
            recursivelyRevealZones(zone, Parameters.Steps);
        }
    }

    private void recursivelyRevealZones(Zone z, int steps)
    {
        if (!Owner.Game.VisibilityLayer.RevealZone(z) || steps <= 0)
        {
            return;
        }

        foreach (var adjacentZone in Owner.Game.ZoneLayer.AdjacentZones(z))
        {
            recursivelyRevealZones(adjacentZone, steps - 1);
        }
    }

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

interface IRevealSurroundingZonesParameters : IParametersTemplate<IRevealSurroundingZonesParameters>
{
    [Modifiable(1)]
    public int Steps { get; }
}

using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drones;

sealed class Drone : Component, ITileWalkerOwner
{
    private readonly DroneRequest request;
    private readonly ImmutableArray<Direction> path;
    private TileWalker? walker;
    private int pathIndex;

    public Drone(DroneRequest request, ImmutableArray<Direction> path)
    {
        this.request = request;
        this.path = path;
    }

    protected override void OnAdded() { }

    public override void Activate()
    {
        base.Activate();
        walker = new TileWalker(this, Owner.Game.Level, Level.GetTile(Owner.Position));
    }

    public override void Update(TimeSpan elapsedTime)
    {
        walker?.Update(elapsedTime, Constants.Game.Drones.Speed);
    }

    public void Cancel()
    {
        Owner.Delete();
    }

    public void OnTileChanged(Tile oldTile, Tile newTile)
    {
        if (newTile != request.Location)
        {
            return;
        }

        request.Action();
        Owner.Delete();
    }

    public Direction GetNextDirection()
    {
        return pathIndex < path.Length ? path[pathIndex++] : Direction.Unknown;
    }
}

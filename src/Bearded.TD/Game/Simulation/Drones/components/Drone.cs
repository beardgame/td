using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drones;

sealed class Drone : Component, ITileWalkerOwner
{
    private readonly DroneRequest request;
    private readonly PrecalculatedPath path;
    private TileWalker? walker;

    public Drone(DroneRequest request, PrecalculatedPath path)
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
        if (walker == null) return;
        walker.Update(elapsedTime, Constants.Game.Drones.Speed);

        var newPosition = walker.Position.WithZ(Constants.Game.Drones.FlyingHeight);
        Owner.Direction = Direction2.Of((newPosition - Owner.Position).NumericValue.Xy);
        Owner.Position = newPosition;
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
        return path.NextDirectionFromTile(walker!.CurrentTile);
    }
}

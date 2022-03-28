using System.Collections.Generic;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Units;

// TODO: split target determination and actual movement management
// TODO: split out tile presence management
[Component("moveToBase")]
sealed class MoveToBase
    : Component<IMoveToBaseParameters>,
        IEnemyMovement,
        IListener<ObjectDeleting>,
        ITileWalkerOwner,
        ISyncable
{
    private TileWalker tileWalker = null!;
    private PassabilityLayer passabilityLayer = null!;

    private Tile currentTile => tileWalker.CurrentTile;

    public Position2 Position => tileWalker.Position;
    public Tile GoalTile => tileWalker.GoalTile;
    public bool IsMoving => tileWalker.IsMoving;

    public IEnumerable<Tile> OccupiedTiles => currentTile.Yield();

    public MoveToBase(IMoveToBaseParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        tileWalker = new TileWalker(this, Owner.Game.Level, Level.GetTile(Owner.Position));
        Owner.Game.UnitLayer.AddEnemyToTile(currentTile, Owner);
        Events.Send(new TileEntered(currentTile));
        passabilityLayer = Owner.Game.PassabilityManager.GetLayer(Passability.WalkingUnit);
        Events.Subscribe(this);
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        Owner.Game.UnitLayer.RemoveEnemyFromTile(currentTile, Owner);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        tileWalker.Update(elapsedTime, Parameters.MovementSpeed);
        Owner.Position = tileWalker.Position.WithZ(Owner.Game.GeometryLayer[currentTile].DrawInfo.Height + .05.U());
    }

    public void OnTileChanged(Tile oldTile, Tile newTile)
    {
        Owner.Game.UnitLayer.MoveEnemyBetweenTiles(oldTile, newTile, Owner);
        Events.Send(new TileLeft(oldTile));
        Events.Send(new TileEntered(newTile));
    }

    public Direction GetNextDirection()
    {
        var desiredDirection = Owner.Game.Navigator.GetDirections(currentTile);

        if (desiredDirection == Direction.Unknown && !passabilityLayer[currentTile].IsPassable)
        {
            // this accounts for getting stuck in building or other changes to level
            desiredDirection = tryToGetUnstuck();
        }

        var isPassable = passabilityLayer[currentTile.Neighbor(desiredDirection)].IsPassable;
        return !isPassable
            ? Direction.Unknown
            : desiredDirection;
    }

    private Direction tryToGetUnstuck()
    {
        return Owner.Game.Navigator.GetDirectionToClosestToSinkNeighbour(currentTile);
    }

    public void Teleport(Position2 pos, Tile tile) => tileWalker.Teleport(pos, tile);

    public IStateToSync GetCurrentStateToSync() =>
        new StateToSync<MoveToBase>(this, new EnemyMovementSynchronizedState(this));
}

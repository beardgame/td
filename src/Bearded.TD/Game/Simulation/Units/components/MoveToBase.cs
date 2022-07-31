using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Units;

// TODO: split target determination and actual movement management
// TODO: split out tile presence management
[Component("moveToBase")]
sealed class MoveToBase
    : Component<MoveToBase.IParameters>,
        IEnemyMovement,
        IListener<ObjectDeleting>,
        ISyncable
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.MovementSpeed)]
        Speed MovementSpeed { get; }
    }

    private PhysicalPresence? presence;

    public bool IsMoving => presence?.IsMoving ?? false;
    public IEnumerable<Tile> OccupiedTiles =>
        presence == null ? Enumerable.Empty<Tile>() : presence.CurrentTile.Yield();

    public MoveToBase(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() {}

    public override void Activate()
    {
        base.Activate();

        presence = new PhysicalPresence(Owner, Events);
        Events.Subscribe(this);
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        presence?.RemoveFromGame();
    }

    public override void Update(TimeSpan elapsedTime)
    {
        presence!.Update(elapsedTime, Parameters.MovementSpeed);
        Owner.Position = presence.OwnerPosition;
    }

    public void Teleport(Position2 pos, Tile tile)
    {
        presence?.Teleport(pos, tile);
    }

    public IStateToSync GetCurrentStateToSync()
    {
        if (presence == null)
        {
            throw new InvalidOperationException("Cannot synchronize an object that does not have a presence.");
        }
        return new StateToSync<MoveToBase>(this, new EnemyMovementSynchronizedState(presence));
    }

    private sealed class PhysicalPresence : IEnemyMovement.IPhysicalPresence, ITileWalkerOwner
    {
        private readonly GameObject owner;
        private readonly ComponentEvents events;
        private readonly TileWalker tileWalker;
        private readonly PassabilityLayer passabilityLayer;

        public Position2 Position => tileWalker.Position;
        public Tile CurrentTile => tileWalker.CurrentTile;
        public Tile GoalTile => tileWalker.GoalTile;
        public bool IsMoving => tileWalker.IsMoving;

        public Position3 OwnerPosition =>
            tileWalker.Position.WithZ(owner.Game.GeometryLayer[CurrentTile].DrawInfo.Height + .05.U());

        public PhysicalPresence(GameObject owner, ComponentEvents events)
        {
            this.owner = owner;
            this.events = events;
            tileWalker = new TileWalker(this, owner.Game.Level, Level.GetTile(owner.Position));
            passabilityLayer = owner.Game.PassabilityManager.GetLayer(Passability.WalkingUnit);
            owner.Game.UnitLayer.AddEnemyToTile(CurrentTile, owner);
            events.Send(new TileEntered(CurrentTile));
        }

        public void OnTileChanged(Tile oldTile, Tile newTile)
        {
            owner.Game.UnitLayer.MoveEnemyBetweenTiles(oldTile, newTile, owner);
            events.Send(new TileLeft(oldTile));
            events.Send(new TileEntered(newTile));
        }

        public Direction GetNextDirection()
        {
            var desiredDirection = owner.Game.Navigator.GetDirections(CurrentTile);

            if (desiredDirection == Direction.Unknown && !passabilityLayer[CurrentTile].IsPassable)
            {
                // this accounts for getting stuck in building or other changes to level
                desiredDirection = tryToGetUnstuck();
            }

            var isPassable = passabilityLayer[CurrentTile.Neighbor(desiredDirection)].IsPassable;
            return !isPassable
                ? Direction.Unknown
                : desiredDirection;
        }

        private Direction tryToGetUnstuck()
        {
            return owner.Game.Navigator.GetDirectionToClosestToSinkNeighbour(CurrentTile);
        }

        public void Update(TimeSpan elapsedTime, Speed currentSpeed)
        {
            tileWalker.Update(elapsedTime, currentSpeed);
        }

        public void Teleport(Position2 newPos, Tile tile)
        {
            tileWalker.Teleport(newPos, tile);
        }

        public void RemoveFromGame()
        {
            owner.Game.UnitLayer.RemoveEnemyFromTile(CurrentTile, owner);
        }
    }
}

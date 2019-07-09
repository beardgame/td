using Bearded.TD.Content.Models;
using Bearded.TD.Game.Navigation;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.EnemyBehavior
{
    // Note: right now this component does both the movement and determining the target. Ideally we want to split that,
    // but right now there is no good way to communicate between components.
    [Component("moveToBase")]
    sealed class MoveToBase
        : Component<EnemyUnit, IMoveToBaseParameters>,
            IEnemyMovement,
            ITileWalkerOwner,
            ISyncable
    {
        private TileWalker tileWalker;
        private PassabilityLayer passabilityLayer;

        public Position2 Position => tileWalker.Position;
        public Tile CurrentTile => tileWalker.CurrentTile;
        public Tile GoalTile => tileWalker.GoalTile;
        public bool IsMoving => tileWalker.IsMoving;

        public MoveToBase(IMoveToBaseParameters parameters) : base(parameters) { }

        protected override void Initialise()
        {
            tileWalker = new TileWalker(this, Owner.Game.Level, Owner.CurrentTile);
            passabilityLayer = Owner.Game.PassabilityManager.GetLayer(Passability.WalkingUnit);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            tileWalker.Update(elapsedTime, Parameters.MovementSpeed);
        }

        public override void Draw(GeometryManager geometries) { }

        public void OnTileChanged(Tile oldTile, Tile newTile)
        {
            Owner.OnTileChanged(oldTile, newTile);
        }

        public Direction GetNextDirection()
        {
            var desiredDirection = Owner.Game.Navigator.GetDirections(CurrentTile);

            if (desiredDirection == Direction.Unknown && !passabilityLayer[CurrentTile].IsPassable)
            {
                // this accounts for getting stuck in building or other changes to level
                desiredDirection = tryToGetUnstuck();
            }

            var isPassable = passabilityLayer[CurrentTile.Neighbour(desiredDirection)].IsPassable;
            return !isPassable
                ? Direction.Unknown
                : desiredDirection;
        }

        private Direction tryToGetUnstuck()
        {
            return Owner.Game.Navigator.GetDirectionToClosestToSinkNeighbour(CurrentTile);
        }

        public void Teleport(Position2 pos, Tile tile) => tileWalker.Teleport(pos, tile);

        public IStateToSync GetCurrentStateToSync() =>
            new StateToSync<MoveToBase>(this, new EnemyMovementSynchronizedState(this));
    }
}

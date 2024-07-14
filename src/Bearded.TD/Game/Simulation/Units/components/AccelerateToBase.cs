using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Units;

[Component("accelerateToBase")]
sealed class AccelerateToBase : Component<AccelerateToBase.IParameters>, IEnemyMovement
{
    private IPhysics physics = null!;
    private PassabilityLayer passabilityLayer = null!;

    public bool IsMoving { get; private set; }
    public Direction TileDirection { get; private set; }

    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.MovementSpeed)]
        Acceleration Acceleration { get; }
    }

    public AccelerateToBase(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IPhysics>(Owner, Events, p => physics = p);
    }

    public override void Activate()
    {
        base.Activate();
        passabilityLayer = Owner.Game.PassabilityObserver.GetLayer(Passability.WalkingUnit);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var tile = Level.GetTile(Owner.Position);
        TileDirection = getBestDirection(tile);

        if (TileDirection == Direction.Unknown)
        {
            IsMoving = false;
            return;
        }

        IsMoving = passabilityLayer[tile.Neighbor(TileDirection)].IsPassable;

        var nextTile = tile.Neighbor(TileDirection);
        var nextTilePosition = Level.GetPosition(nextTile);

        var direction = (nextTilePosition - Owner.Position.XY()).NumericValue.NormalizedSafe();
        var acceleration = direction * Parameters.Acceleration;

        acceleration *= IsMoving ? 1 : 0.1f;

        physics.ApplyVelocityImpulse((acceleration * elapsedTime).WithZ());
    }

    private Direction getBestDirection(Tile tile)
    {
        var desiredDirection = Owner.Game.Navigator.GetDirectionToSink(tile);

        if (desiredDirection == Direction.Unknown && !passabilityLayer[tile].IsPassable)
        {
            // this accounts for getting stuck in building or other changes to level
            desiredDirection = tryToGetUnstuck(tile);
        }

        return desiredDirection;
    }

    private Direction tryToGetUnstuck(Tile tile)
    {
        return Owner.Game.Navigator.GetDirectionToClosestToSinkNeighbour(tile);
    }
}

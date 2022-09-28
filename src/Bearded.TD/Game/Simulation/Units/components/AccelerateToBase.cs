using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Footprints;
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

    public interface IParameters : IParametersTemplate<IParameters>
    {
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
        passabilityLayer = Owner.Game.PassabilityManager.GetLayer(Passability.WalkingUnit);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var tile = Level.GetTile(Owner.Position);
        var nextTileDirection = getBestDirection(tile);

        IsMoving = nextTileDirection != Direction.Unknown;
        if (!IsMoving)
            return;

        var nextTile = tile.Neighbor(nextTileDirection);
        var nextTilePosition = Level.GetPosition(nextTile);

        var direction = (nextTilePosition - Owner.Position.XY()).NumericValue.NormalizedSafe();
        var acceleration = direction * Parameters.Acceleration;

        physics.ApplyVelocityImpulse((acceleration * elapsedTime).WithZ());
    }

    private Direction getBestDirection(Tile tile)
    {
        var desiredDirection = Owner.Game.Navigator.GetDirections(tile);

        if (desiredDirection == Direction.Unknown && !passabilityLayer[tile].IsPassable)
        {
            // this accounts for getting stuck in building or other changes to level
            desiredDirection = tryToGetUnstuck(tile);
        }

        var isPassable = passabilityLayer[tile.Neighbor(desiredDirection)].IsPassable;
        return !isPassable
            ? Direction.Unknown
            : desiredDirection;
    }

    private Direction tryToGetUnstuck(Tile tile)
    {
        return Owner.Game.Navigator.GetDirectionToClosestToSinkNeighbour(tile);
    }

    IEnumerable<Tile> ITileOccupation.OccupiedTiles => Enumerable.Empty<Tile>();
    void IEnemyMovement.Teleport(Position2 pos, Tile tile) { }
}

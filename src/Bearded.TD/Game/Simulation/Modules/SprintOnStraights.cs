using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

readonly record struct StartedSprinting(Direction Direction) : IComponentEvent;
readonly record struct StoppedSprinting(Direction Direction) : IComponentEvent;

[Component("sprintOnStraights")]
sealed class SprintOnStraights : Component<SprintOnStraights.IParameters>
{
    private IEnemyMovement movement = null!;
    private IPhysics physics = null!;

    private Direction currentDirection = Direction.Unknown;
    private Instant lastDirectionChange;
    private bool sprinting;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        TimeSpan TimeToSprint { get; }
        Acceleration MinAcceleration { get; }
        TimeSpan TimeToMaxAcceleration { get; }
        Acceleration MaxAcceleration { get; }
    }

    public SprintOnStraights(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IEnemyMovement>(Owner, Events, m => movement = m);
        ComponentDependencies.Depend<IPhysics>(Owner, Events, p => physics = p);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var newDirection = movement.TileDirection;

        if (!movement.IsMoving || newDirection == Direction.Unknown || newDirection != currentDirection)
        {
            stopSprinting();
            resetTime();
        }
        else if (Owner.Game.Time - lastDirectionChange > Parameters.TimeToSprint)
        {
            startSprinting(newDirection);
        }

        currentDirection = newDirection;

        if (!sprinting)
            return;

        var timeSinceSprintStart = Owner.Game.Time - lastDirectionChange - Parameters.TimeToSprint;
        var accelerationInterpolator = timeSinceSprintStart / Parameters.TimeToMaxAcceleration;

        var accelerationMagnitude = Interpolate.Lerp(
            Parameters.MinAcceleration.NumericValue,
            Parameters.MaxAcceleration.NumericValue,
            (float)accelerationInterpolator
        );

        var acceleration = new Acceleration3(currentDirection.Vector().WithZ() * accelerationMagnitude);

        physics.ApplyVelocityImpulse(acceleration * elapsedTime);
    }

    private void startSprinting(Direction direction)
    {
        if (sprinting)
            return;

        sprinting = true;
        Events.Send(new StartedSprinting(direction));
    }

    private void resetTime()
    {
        lastDirectionChange = Owner.Game.Time;
    }

    private void stopSprinting()
    {
        if (!sprinting)
            return;

        sprinting = false;
        Events.Send(new StoppedSprinting(currentDirection));
    }
}


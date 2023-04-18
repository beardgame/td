using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Units;

[Component("notifyWhenStuck")]
sealed class NotifyWhenStuck : Component<NotifyWhenStuck.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(0.5)]
        TimeSpan StuckDelay { get; }
    }

    private IEnemyMovement? movement;
    private Instant? stuckNotificationTime;
    private bool wasStuck;

    public NotifyWhenStuck(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IEnemyMovement>(Owner, Events, m => movement = m);
    }

    public override void Activate() { }

    public override void Update(TimeSpan elapsedTime)
    {
        var stoppedMoving = movement is { IsMoving: false };

        if (wasStuck == stoppedMoving)
        {
            stuckNotificationTime = null;
            return;
        }

        if (stuckNotificationTime is { } time)
        {
            if (time <= Owner.Game.Time)
            {
                notifyStuck();
            }

            return;
        }

        if (stoppedMoving)
        {
            queueNotification();
        }
        else
        {
            notifyUnstuck();
        }
    }

    private void queueNotification()
    {
        if (Parameters.StuckDelay <= TimeSpan.Zero)
        {
            notifyStuck();
            return;
        }

        stuckNotificationTime = Owner.Game.Time + Parameters.StuckDelay;
    }

    private void notifyStuck()
    {
        if (movement is null)
        {
            throw new InvalidOperationException("Movement cannot be set to null after initiating stuck notification");
        }

        var tile = Level.GetTile(Owner.Position);
        var target = tile.Neighbor(movement.TileDirection);
        Events.Send(new EnemyGotStuck(target));
        wasStuck = true;
        stuckNotificationTime = null;
    }

    private void notifyUnstuck()
    {
        Events.Send(new EnemyGotUnstuck());
        wasStuck = false;
    }
}

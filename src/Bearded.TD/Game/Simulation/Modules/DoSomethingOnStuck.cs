using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

abstract class DoSomethingOnStuck<T> : Component<T> where T : IParametersTemplate<T>
{
    private bool actionDone;
    private IEnemyMovement? movement;
    private Instant? activationTime;

    protected abstract TimeSpan Delay { get; }

    protected DoSomethingOnStuck(T parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IEnemyMovement>(Owner, Events, m => movement = m);
    }

    public override void Activate() { }

    public override void Update(TimeSpan elapsedTime)
    {
        if (actionDone) return;

        if (activationTime is { } time && time <= Owner.Game.Time)
        {
            doActionImmediately();
            return;
        }

        var stoppedMoving = movement is { IsMoving: false };
        var hasActionQueued = activationTime != null;

        switch (stoppedMoving)
        {
            case true when !hasActionQueued:
                triggerAction();
                break;
            case false:
                activationTime = null;
                break;
        }
    }

    private void triggerAction()
    {
        if (Delay > TimeSpan.Zero)
        {
            queueAction();
            return;
        }

        doActionImmediately();
    }

    private void queueAction()
    {
        activationTime = Owner.Game.Time + Delay;
    }

    private void doActionImmediately()
    {
        if (movement is not null)
        {
            var tile = Level.GetTile(Owner.Position);
            var target = tile.Neighbor(movement.TileDirection);
            DoAction(target);
        }

        Events.Send(new EnactDeath());
        actionDone = true;
    }

    protected abstract void DoAction(Tile targetTile);
}

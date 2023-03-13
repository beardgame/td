using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop;

[Component("gameOverOnDestroy")]
sealed class GameOverOnDestroy : Component, IListener<ObjectKilled>
{
    private bool eventSent;

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public void HandleEvent(ObjectKilled @event)
    {
        Owner.Sync(LoseGame.Command);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        // Wait for the first update loop to ensure all components were added.
        // Ugly but... well.
        if (eventSent) return;
        Events.Send(new PreventPlayerHealthChanges());
        Events.Send(new PreventRuin());
        eventSent = true;
    }
}

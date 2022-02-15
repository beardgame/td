using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop;

[Component("gameOverOnDestroy")]
sealed class GameOverOnDestroy<T> : Component<T>
    where T : GameObject
{
    private bool eventSent;

    protected override void OnAdded()
    {
        Owner.Deleting += onDeleting;
    }

    private void onDeleting()
    {
        Owner.Sync(LoseGame.Command);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        // Wait for the first update loop to ensure all components were added.
        // Ugly but... well.
        if (eventSent) return;
        Events.Send(new PreventPlayerHealthChanges());
        eventSent = true;
    }
}

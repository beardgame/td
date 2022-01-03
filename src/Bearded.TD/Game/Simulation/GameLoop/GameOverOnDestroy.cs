using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop;

[Component("gameOverOnDestroy")]
class GameOverOnDestroy<T> : Component<T>
    where T : GameObject
{
    protected override void OnAdded()
    {
        Owner.Deleting += onDeleting;
    }

    private void onDeleting()
    {
        Owner.Sync(LoseGame.Command);
    }

    public override void Update(TimeSpan elapsedTime) { }
}
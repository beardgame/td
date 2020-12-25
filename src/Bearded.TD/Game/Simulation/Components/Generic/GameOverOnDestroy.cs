using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components.Generic
{
    [Component("gameOverOnDestroy")]
    class GameOverOnDestroy<T> : Component<T>
        where T : GameObject
    {
        protected override void Initialise()
        {
            Owner.Deleting += onDeleting;
        }

        private void onDeleting()
        {
            Owner.Sync(GameOver.Command);
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(CoreDrawers geometries) { }
    }
}

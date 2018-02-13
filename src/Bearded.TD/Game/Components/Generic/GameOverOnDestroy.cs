using Bearded.TD.Game.Commands;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("gameOverOnDestroy")]
    class GameOverOnDestroy<T> : Component<T>
        where T : GameObject, IDamageable
    {
        protected override void Initialise()
        {
            Owner.Damaged += onDamaged;
        }

        private void onDamaged()
        {
            if (Owner.Health <= 0)
            {
                Owner.Sync(GameOver.Command);
            }
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(GeometryManager geometries) { }
    }
}

using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings.Components
{
    class GameOverOnDestroy : Component
    {
        protected override void Initialise()
        {
            Building.Damaged += onDamaged;
        }

        private void onDamaged()
        {
            if (Building.Health <= 0)
                Building.Sync(GameOver.Command);
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(GeometryManager geometries) { }
    }
}

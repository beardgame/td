using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Damage
{
    sealed class HealthBar<T> : Component<T>, IDrawableComponent
        where T : IPositionable, IComponentOwner
    {
        private IHealth health = null!;

        protected override void OnAdded()
        {
            ComponentDependencies.Depend<IHealth>(Owner, Events, h => health = h);
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(CoreDrawers drawers)
        {
        }

        public void Draw(IComponentRenderer renderer)
        {
            var p = (float) health.HealthPercentage;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            // given the implementation of current/total, this is guaranteed by IEEE754
            if (p == 1)
                return;

            var healthColor = Color.FromHSVA(Interpolate.Lerp(Color.Red.Hue, Color.Green.Hue, p), .8f, .8f);

            var topLeft = Owner.Position.NumericValue - new Vector3(.5f, .5f, 0);
            var size = new Vector2(1, .1f);

            var drawer = renderer.Core.ConsoleBackground;

            drawer.FillRectangle(topLeft, size, Color.DarkGray);
            drawer.FillRectangle(topLeft, new Vector2(size.X * p, size.Y), healthColor);
        }
    }
}

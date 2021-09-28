using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements
{
    [Component("initialSpark")]
    class InitialSpark<T> : Component<T>
    {
        private bool sparked;

        protected override void OnAdded()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (sparked)
                return;

            Events.Send(new Spark());
            sparked = true;
        }

        public override void Draw(CoreDrawers drawers)
        {
        }
    }
}

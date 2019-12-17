using Bearded.TD.Game.Components.Events;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Fire
{
    [Component("initialSpark")]
    class InitialSpark<T> : Component<T>
        where T : IComponentEventManager
    {
        private bool sparked;

        protected override void Initialise()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (sparked)
                return;

            Owner.Events.Send(new Spark());
            sparked = true;
        }

        public override void Draw(GeometryManager geometries)
        {
        }
    }
}

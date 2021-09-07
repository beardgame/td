using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements
{
    [Component("deleteOnExtinguish")]
    class DeleteOnExtinguish<T> : Component<T>,  IListener<FireExtinguished>
        where T : GameObject
    {
        protected override void Initialize()
        {
            Events.Subscribe(this);
        }

        public void HandleEvent(FireExtinguished @event)
        {
            Owner.Delete();
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(CoreDrawers drawers)
        {
        }
    }
}

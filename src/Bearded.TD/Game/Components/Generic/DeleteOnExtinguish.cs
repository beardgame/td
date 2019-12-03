using Bearded.TD.Game.Components.Events;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("deleteOnExtinguish")]
    class DeleteOnExtinguish<T> : Component<T>,  IListener<FireExtinguished>
        where T : GameObject, IEventManager
    {
        protected override void Initialise()
        {
            Owner.Events.Subscribe(this);
        }

        public void HandleEvent(FireExtinguished @event)
        {
            Owner.Delete();
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(GeometryManager geometries)
        {
        }
    }
}

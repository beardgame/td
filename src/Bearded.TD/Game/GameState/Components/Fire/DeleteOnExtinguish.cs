using Bearded.TD.Game.GameState.Events;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameState.Components.Fire
{
    [Component("deleteOnExtinguish")]
    class DeleteOnExtinguish<T> : Component<T>,  IListener<FireExtinguished>
        where T : GameObject
    {
        protected override void Initialise()
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

        public override void Draw(GeometryManager geometries)
        {
        }
    }
}

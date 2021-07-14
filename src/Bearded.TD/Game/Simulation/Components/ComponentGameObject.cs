using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.Utilities.Maybe;

namespace Bearded.TD.Game.Simulation.Components
{
    [ComponentOwner]
    sealed class ComponentGameObject : GameObject, IComponentOwner<ComponentGameObject>, IPositionable, IDirected
    {
        private readonly IComponentOwnerBlueprint blueprint;
        public Maybe<IComponentOwner> Parent { get; }
        public Position3 Position { get; set; }
        public Direction2 Direction { get; private set; }

        private readonly ComponentCollection<ComponentGameObject> components;
        private readonly ComponentEvents events = new();

        public ComponentGameObject(
            IComponentOwnerBlueprint blueprint, IComponentOwner? parent, Position3 position, Direction2 direction)
        {
            this.blueprint = blueprint;
            Direction = direction;
            Parent = FromNullable(parent);
            Position = position;
            components = new ComponentCollection<ComponentGameObject>(this, events);
        }

        protected override void OnAdded()
        {
            components.Add(blueprint.GetComponents<ComponentGameObject>());
        }

        protected override void OnDelete()
        {
            events.Send(new ObjectDeleting());
            base.OnDelete();
        }

        public override void Update(TimeSpan elapsedTime)
        {
            components.Update(elapsedTime);
        }

        public override void Draw(CoreDrawers drawers)
        {
            components.Draw(drawers);
        }

        IEnumerable<TComponent> IComponentOwner<ComponentGameObject>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<T> IComponentOwner.GetComponents<T>() => components.Get<T>();
    }
}

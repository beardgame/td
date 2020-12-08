using System.Collections.Generic;
using Bearded.TD.Game.GameState.Components.Events;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.Utilities.Maybe;

namespace Bearded.TD.Game.GameState.Components
{
    [ComponentOwner]
    class ComponentGameObject : GameObject, IComponentOwner<ComponentGameObject>, IPositionable, IDirected
    {
        private readonly IComponentOwnerBlueprint blueprint;
        public Maybe<IComponentOwner> Parent { get; }
        public Position3 Position { get; private set; }
        public Direction2 Direction { get; private set; }

        private readonly ComponentCollection<ComponentGameObject> components;
        private readonly ComponentEvents events = new ComponentEvents();

        public ComponentGameObject(IComponentOwnerBlueprint blueprint, IComponentOwner parent, Position3 position, Direction2 direction)
        {
            this.blueprint = blueprint;
            Direction = direction;
            Parent = Just(parent);
            Position = position;
            components = new ComponentCollection<ComponentGameObject>(this, events);
        }

        protected override void OnAdded()
        {
            components.Add(blueprint.GetComponents<ComponentGameObject>());
        }

        public override void Update(TimeSpan elapsedTime)
        {
            components.Update(elapsedTime);
        }

        public override void Draw(GeometryManager geometries)
        {
            components.Draw(geometries);
        }

        IEnumerable<TComponent> IComponentOwner<ComponentGameObject>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<T> IComponentOwner.GetComponents<T>() => components.Get<T>();
    }
}
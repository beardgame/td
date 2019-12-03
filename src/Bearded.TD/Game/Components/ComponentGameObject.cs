using System.Collections.Generic;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components
{
    [ComponentOwner]
    class ComponentGameObject : GameObject, IComponentOwner<ComponentGameObject>, IEventManager, IPositionable
    {
        private readonly IComponentOwnerBlueprint blueprint;
        public Position3 Position { get; private set; }

        public GameEvents Events { get; } = new GameEvents();

        private readonly ComponentCollection<ComponentGameObject> components;

        public ComponentGameObject(IComponentOwnerBlueprint blueprint, Position3 position)
        {
            this.blueprint = blueprint;
            Position = position;
            components = new ComponentCollection<ComponentGameObject>(this);
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

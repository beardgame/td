using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.Utilities.Maybe;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Projectiles
{
    [ComponentOwner]
    sealed class Projectile : GameObject, IPositionable, IComponentOwner<Projectile>
    {
        private readonly IComponentOwnerBlueprint blueprint;
        private readonly ComponentCollection<Projectile> components;
        private readonly ComponentEvents events = new();

        public Maybe<IComponentOwner> Parent => Nothing;

        public Position3 Position { get; set; }

        public Projectile(IComponentOwnerBlueprint blueprint, Position3 position)
        {
            this.blueprint = blueprint;
            Position = position;

            components = new ComponentCollection<Projectile>(this, events);
        }

        protected override void OnAdded()
        {
            components.Add(blueprint.GetComponents<Projectile>());
        }
        public override void Update(TimeSpan elapsedTime)
        {
            components.Update(elapsedTime);
        }

        public override void Draw(CoreDrawers drawers)
        {
            components.Draw(drawers);
        }

        public void AddComponent(IComponent<Projectile> component) => components.Add(component);

        public void RemoveComponent(IComponent<Projectile> component) => components.Remove(component);

        IEnumerable<TComponent> IComponentOwner<Projectile>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<T> IComponentOwner.GetComponents<T>() => components.Get<T>();
    }
}

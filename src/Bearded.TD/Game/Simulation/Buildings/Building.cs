using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [ComponentOwner]
    sealed class Building
        : GameObject,
            IComponentGameObject,
            IBuilding,
            IComponentOwner<Building>,
            IPositionable
    {
        private readonly ComponentCollection<Building> components;
        private readonly ComponentEvents events = new();
        private readonly IBuildingBlueprint blueprint;

        public IComponentOwner? Parent => null;

        public Position3 Position { get; set; }

        public Building(IBuildingBlueprint blueprint)
        {
            this.blueprint = blueprint;
            components = new ComponentCollection<Building>(this, events);
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            components.Add(blueprint.GetComponents());
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

        public void AddComponent(IComponent<Building> component) => components.Add(component);

        public void RemoveComponent(IComponent<Building> component) => components.Remove(component);

        public IEnumerable<TComponent> GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<TComponent> IComponentOwner<Building>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<TComponent> IComponentOwner.GetComponents<TComponent>() => components.Get<TComponent>();
    }
}

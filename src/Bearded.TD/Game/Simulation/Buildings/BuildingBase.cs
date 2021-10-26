using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    abstract class BuildingBase<T>
        : GameObject,
            IComponentGameObject,
            IBuilding,
            IComponentOwner<T>,
            IPositionable
        where T : BuildingBase<T>
    {
        private readonly ComponentCollection<T> components;
        protected ComponentEvents Events { get; } = new();

        public IComponentOwner? Parent => null;

        protected IBuildingBlueprint Blueprint { get; }

        public Position3 Position { get; set; }

        protected BuildingBase(IBuildingBlueprint blueprint)
        {
            Blueprint = blueprint;
            components = new ComponentCollection<T>((T) this, Events);
        }

        protected abstract IEnumerable<IComponent<T>> InitializeComponents();

        protected override void OnAdded()
        {
            base.OnAdded();
            components.Add(InitializeComponents());
        }

        protected override void OnDelete()
        {
            Events.Send(new ObjectDeleting());
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

        public void AddComponent(IComponent<T> component) => components.Add(component);

        public void RemoveComponent(IComponent<T> component) => components.Remove(component);

        public IEnumerable<TComponent> GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<TComponent> IComponentOwner<T>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<TComponent> IComponentOwner.GetComponents<TComponent>() => components.Get<TComponent>();
    }
}

using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    abstract class BuildingBase<T>
        : GameObject,
            IBuilding,
            IComponentOwner<T>,
            IFactioned,
            IListener<FootprintChanged>,
            IPositionable
        where T : BuildingBase<T>
    {
        private readonly ComponentCollection<T> components;
        protected ComponentEvents Events { get; } = new();

        public Maybe<IComponentOwner> Parent => Maybe.Nothing;
        public abstract IBuildingState State { get; }

        protected IBuildingBlueprint Blueprint { get; }

        public Faction Faction { get; }
        public Position3 Position { get; private set; }

        protected BuildingBase(
            IBuildingBlueprint blueprint,
            Faction faction)
        {
            Blueprint = blueprint;
            Faction = faction;
            components = new ComponentCollection<T>((T) this, Events);
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            Events.Subscribe(this);
            components.Add(InitializeComponents());
        }

        public void HandleEvent(FootprintChanged @event)
        {
            calculatePosition(@event.NewFootprint);
        }

        private void calculatePosition(PositionedFootprint footprint)
        {
            var z = Game.Level.IsValid(footprint.RootTile)
                ? Game.GeometryLayer[footprint.RootTile].DrawInfo.Height
                : Unit.Zero;
            Position = footprint.CenterPosition.WithZ(z);
        }

        protected override void OnDelete()
        {
            Events.Send(new ObjectDeleting());
            base.OnDelete();
        }

        public void AddComponent(IComponent<T> component)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            DebugAssert.State.Satisfies(Game != null, "Cannot add components before adding the game object to a game.");
            components.Add(component);
        }

        protected abstract IEnumerable<IComponent<T>> InitializeComponents();
        public IEnumerable<TComponent> GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<TComponent> IComponentOwner<T>.GetComponents<TComponent>() => GetComponents<TComponent>();

        IEnumerable<TComponent> IComponentOwner.GetComponents<TComponent>() => components.Get<TComponent>();

        public override void Update(TimeSpan elapsedTime)
        {
            components.Update(elapsedTime);
        }

        public override void Draw(CoreDrawers drawers)
        {
            components.Draw(drawers);
        }
    }
}

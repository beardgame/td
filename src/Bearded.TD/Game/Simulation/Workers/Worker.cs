using System.Collections.Generic;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Workers
{
    [ComponentOwner]
    sealed class Worker : GameObject, IComponentOwner<Worker>, ISelectable
    {
        private readonly ComponentEvents events = new();
        private readonly ComponentCollection<Worker> components;

        // TODO: this should be the worker hub
        public Maybe<IComponentOwner> Parent { get; } = Maybe.Nothing;

        public IFactioned HubOwner { get; }

        public Position2 Position { get; set; } = Position2.Zero;
        public Tile CurrentTile { get; set; }

        public SelectionState SelectionState { get; private set; }

        public Worker(IFactioned hubOwner)
        {
            components = new ComponentCollection<Worker>(this, events);
            HubOwner = hubOwner;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            components.Add(new WorkerComponent());
            Game.ListAs(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            components.Update(elapsedTime);
        }

        public override void Draw(CoreDrawers drawers)
        {
            components.Draw(drawers);
        }

        public void ResetSelection()
        {
            SelectionState = SelectionState.Default;
        }

        public void Focus(SelectionManager selectionManager) {}

        public void Select(SelectionManager selectionManager)
        {
            selectionManager.CheckCurrentlySelected(this);
            SelectionState = SelectionState.Selected;
        }

        IEnumerable<TComponent> IComponentOwner<Worker>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<TComponent> IComponentOwner.GetComponents<TComponent>() => components.Get<TComponent>();
    }
}

using System.Collections.Generic;
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
    sealed class Worker : GameObject, IComponentOwner<Worker>
    {
        private readonly ComponentEvents events = new();
        private readonly ComponentCollection<Worker> components;

        // TODO: this should be the worker hub
        public Maybe<IComponentOwner> Parent { get; } = Maybe.Nothing;

        public IFactioned HubOwner { get; }

        public Position2 Position { get; set; } = Position2.Zero;
        public Tile CurrentTile { get; set; }

        public Worker(IFactioned hubOwner)
        {
            components = new ComponentCollection<Worker>(this, events);
            HubOwner = hubOwner;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            components.Add(new WorkerComponent());
        }

        public override void Update(TimeSpan elapsedTime)
        {
            components.Update(elapsedTime);
        }

        public override void Draw(CoreDrawers drawers)
        {
            components.Draw(drawers);
        }

        IEnumerable<TComponent> IComponentOwner<Worker>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<TComponent> IComponentOwner.GetComponents<TComponent>() => components.Get<TComponent>();
    }
}

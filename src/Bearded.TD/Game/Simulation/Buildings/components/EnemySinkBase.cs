using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings
{
    abstract class EnemySinkBase<T> : Component<T>, IEnemySink, IListener<Materialized>
        where T : IComponentOwner
    {
        private readonly OccupiedTilesTracker occupiedTilesTracker = new();

        protected override void OnAdded()
        {
            Events.Subscribe(this);
        }

        protected abstract void AddSink(Tile t);
        protected abstract void RemoveSink(Tile t);

        public void HandleEvent(Materialized @event)
        {
            occupiedTilesTracker.Initialize(Owner, Events);

            foreach (var tile in occupiedTilesTracker.OccupiedTiles)
            {
                AddSink(tile);
            }

            occupiedTilesTracker.TileAdded += AddSink;
            occupiedTilesTracker.TileRemoved += RemoveSink;
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(CoreDrawers drawers) { }
    }

    interface IEnemySink {}
}

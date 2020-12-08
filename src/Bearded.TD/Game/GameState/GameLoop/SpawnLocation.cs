using System.Collections.Generic;
using Bearded.TD.Game.GameState.Events;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameState.GameLoop
{
    sealed class SpawnLocation : GameObject, IIdable<SpawnLocation>, IListener<WaveEnded>
    {
        private readonly HashSet<Id<WaveScript>> assignedWaves = new();
        private Instant nextIndicatorSpawn;

        public Id<SpawnLocation> Id { get; }
        public Tile Tile { get; }

        public SpawnLocation(Id<SpawnLocation> id, Tile tile)
        {
            Id = id;
            Tile = tile;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.IdAs(this);
            Game.ListAs(this);
            Game.Meta.Events.Subscribe(this);
        }

        public void AssignWave(Id<WaveScript> wave)
        {
            assignedWaves.Add(wave);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (assignedWaves.Count == 0)
            {
                return;
            }

            if (Game.Time >= nextIndicatorSpawn)
            {
                Game.Add(new EnemyPathIndicator(Tile));
                nextIndicatorSpawn = Game.Time + Constants.Game.Enemy.TimeBetweenIndicators;
            }
        }

        public override void Draw(GeometryManager geometries)
        {
        }

        public void HandleEvent(WaveEnded @event)
        {
            assignedWaves.Remove(@event.WaveId);
        }
    }
}

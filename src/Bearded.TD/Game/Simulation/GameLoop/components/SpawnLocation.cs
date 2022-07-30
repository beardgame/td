using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed class SpawnLocation : Component, IIdable<SpawnLocation>, IListener<WaveEnded>, IDeletable
{
    private readonly HashSet<Id<WaveScript>> assignedWaves = new();
    private Instant nextIndicatorSpawn;

    public Id<SpawnLocation> Id { get; }
    public Tile Tile { get; }
    public bool IsAwake { get; private set; }

    public bool Deleted => Owner.Deleted;

    public SpawnLocation(Id<SpawnLocation> id, Tile tile)
    {
        Id = id;
        Tile = tile;
    }

    protected override void OnAdded() {}

    public override void Activate()
    {
        base.Activate();
        Owner.Game.IdAs(Id, this);
        Owner.Game.ListAs(this);
        Owner.Game.Meta.Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Owner.Game.Meta.Events.Unsubscribe(this);
        Owner.Game.DeleteId(Id);
    }

    public void WakeUp()
    {
        IsAwake = true;
    }

    public void AssignWave(Id<WaveScript> wave)
    {
        State.Satisfies(IsAwake);
        assignedWaves.Add(wave);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (assignedWaves.Count == 0)
        {
            return;
        }

        if (Owner.Game.Time >= nextIndicatorSpawn)
        {
            GameLoopObjectFactory.CreateEnemyPathIndicator(Owner.Game, Tile);
            nextIndicatorSpawn = Owner.Game.Time + Constants.Game.Enemy.TimeBetweenIndicators;
        }
    }

    public void HandleEvent(WaveEnded @event)
    {
        assignedWaves.Remove(@event.WaveId);
    }
}

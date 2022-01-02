using System.Collections.Generic;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed class SpawnLocation : GameObject, IIdable<SpawnLocation>, IListener<WaveEnded>
{
    private readonly HashSet<Id<WaveScript>> assignedWaves = new();
    private Instant nextIndicatorSpawn;

    public Id<SpawnLocation> Id { get; }
    public Tile Tile { get; }
    public bool IsAwake { get; private set; }

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

    protected override void OnDelete()
    {
        Game.Meta.Events.Unsubscribe(this);
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

        if (Game.Time >= nextIndicatorSpawn)
        {
            Game.Add(new EnemyPathIndicator(Tile));
            nextIndicatorSpawn = Game.Time + Constants.Game.Enemy.TimeBetweenIndicators;
        }
    }

    public void HandleEvent(WaveEnded @event)
    {
        assignedWaves.Remove(@event.WaveId);
    }
}

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
    private readonly Tile tile;
    private readonly HashSet<Id<WaveScript>> assignedWaves = new();
    private GameObject? placeholder;

    public Id<SpawnLocation> Id { get; }
    public Tile SpawnTile { get; private set; }
    public bool IsAwake { get; private set; }

    public bool Deleted => Owner.Deleted;

    public SpawnLocation(Id<SpawnLocation> id, Tile tile)
    {
        Id = id;
        this.tile = tile;
        SpawnTile = tile;
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

    public void UpdateSpawnTile()
    {
        State.Satisfies(assignedWaves.Count == 0, "Cannot update the tile to spawn on while waves are assigned.");

        // TODO: implement
    }

    public void AssignWave(Id<WaveScript> wave)
    {
        State.Satisfies(IsAwake);
        assignedWaves.Add(wave);

        if (placeholder == null)
        {
            createSpawnPlaceholder();
        }
    }

    private void createSpawnPlaceholder()
    {
        State.Satisfies(placeholder == null);
        placeholder = GameLoopObjectFactory.CreateSpawnPlaceholder(Owner, SpawnTile);
        Owner.Game.Add(placeholder);
    }

    public void HandleEvent(WaveEnded @event)
    {
        assignedWaves.Remove(@event.WaveId);

        if (assignedWaves.Count > 0)
        {
            return;
        }

        placeholder?.Delete();
        placeholder = null;
    }

    public override void Update(TimeSpan elapsedTime) {}
}

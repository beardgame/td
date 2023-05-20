using System;
using Bearded.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.UI.Controls;

sealed class GameStatusUI : IListener<WaveScheduled>, IListener<WaveStarted>, IListener<WaveEnded>
{
    public event VoidEventHandler? StatusChanged;

    private GameInstance game = null!;
    private Faction faction = null!;
    private FactionResources? resources;

    private Id<Wave> currentWave;
    private Instant? waveSpawnStart;
    private Func<bool>? canSummonCurrentWaveNow;

    public string? WaveName { get; private set; }

    public string FactionName => faction.Name;
    public Color FactionColor => faction.Color;
    public ResourceAmount FactionResources => resources?.CurrentResources ?? ResourceAmount.Zero;
    public ResourceAmount FactionResourcesAfterReservation => resources?.ResourcesAfterQueue ?? ResourceAmount.Zero;
    public TimeSpan? TimeUntilWaveSpawn => waveSpawnStart == null ? null : waveSpawnStart - game.State.Time;
    public bool CanSummonNow => canSummonCurrentWaveNow?.Invoke() ?? false;

    public void Initialize(GameInstance game)
    {
        this.game = game;

        faction = game.Me.Faction;
        faction.TryGetBehaviorIncludingAncestors(out resources);

        game.Meta.Events.Subscribe<WaveScheduled>(this);
        game.Meta.Events.Subscribe<WaveStarted>(this);
        game.Meta.Events.Subscribe<WaveEnded>(this);
    }

    public void Update()
    {
        StatusChanged?.Invoke();
    }

    public void HandleEvent(WaveScheduled @event)
    {
        currentWave = @event.WaveId;
        WaveName = @event.WaveName;
        waveSpawnStart = @event.SpawnStart;
        canSummonCurrentWaveNow = @event.CanSummonNow;
    }

    public void HandleEvent(WaveStarted @event)
    {
        if (@event.WaveId != currentWave)
        {
            return;
        }

        waveSpawnStart = null;
        canSummonCurrentWaveNow = null;
    }

    public void HandleEvent(WaveEnded @event)
    {
        if (@event.WaveId != currentWave)
        {
            return;
        }

        currentWave = Id<Wave>.Invalid;
        WaveName = null;
        waveSpawnStart = null;
        canSummonCurrentWaveNow = null;
    }

    public void SkipWaveTimer()
    {
        game.Request(Game.Simulation.GameLoop.SkipWaveTimer.Request);
    }
}

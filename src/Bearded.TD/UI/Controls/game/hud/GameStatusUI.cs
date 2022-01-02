using Bearded.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.UI.Controls;

sealed class GameStatusUI : IListener<WaveScheduled>, IListener<WaveStarted>, IListener<WaveEnded>
{
    public event VoidEventHandler? StatusChanged;

    private GameInstance game = null!;
    private Faction faction = null!;
    private FactionResources? resources;
    private FactionTechnology? technology;

    private Id<WaveScript> currentWave;
    private Instant? waveSpawnStart;

    public string? WaveName { get; private set; }
    public ResourceAmount? WaveResources { get; private set; }

    public string FactionName => faction.Name;
    public Color FactionColor => faction.Color;
    public ResourceAmount FactionResources => resources?.CurrentResources ?? ResourceAmount.Zero;
    public ResourceAmount FactionResourcesAfterReservation => resources?.ResourcesAfterQueue ?? ResourceAmount.Zero;
    public long FactionTechPoints => technology?.TechPoints ?? 0;
    public TimeSpan? TimeUntilWaveSpawn =>
        waveSpawnStart == null
            ? null
            : waveSpawnStart - game.State.Time;

    public void Initialize(GameInstance game)
    {
        this.game = game;

        faction = game.Me.Faction;
        faction.TryGetBehaviorIncludingAncestors(out resources);
        faction.TryGetBehaviorIncludingAncestors(out technology);

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
        WaveResources = @event.ResourceAmount;
    }

    public void HandleEvent(WaveStarted @event)
    {
        if (@event.WaveId != currentWave)
        {
            return;
        }

        waveSpawnStart = null;
    }

    public void HandleEvent(WaveEnded @event)
    {
        if (@event.WaveId != currentWave)
        {
            return;
        }

        currentWave = Id<WaveScript>.Invalid;
        WaveName = null;
        waveSpawnStart = null;
        WaveResources = null;
    }

    public void SkipWaveTimer()
    {
        game.Request(Game.Commands.Gameplay.SkipWaveTimer.Request);
    }
}
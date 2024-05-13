using System;
using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Shared.Events;
using Bearded.TD.UI.Shortcuts;
using Bearded.TD.Utilities;
using Bearded.UI.EventArgs;
using Bearded.Utilities.SpaceTime;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.UI.Controls;

sealed class CoreStatsUI : IListener<WaveScheduled>, IListener<WaveStarted>, IListener<WaveEnded>
{
    private readonly ShortcutLayer shortcuts;

    private GameInstance? gameInstance;
    private ShortcutCapturer? shortcutCapturer;
    private ICoreStats? coreStats;
    private UpcomingWaveInfo? upcomingWaveInfo;

    public Binding<bool> Visible { get; } = new();
    public Binding<CoreHealthStats> Health { get; } = new();
    public Binding<bool> EMPAvailable { get; } = new();
    public Binding<GamePhase> CurrentPhase { get; } = new(GamePhase.BetweenWaves);
    public Binding<UpcomingWaveCountdown?> UpcomingWave { get; } = new();

    public CoreStatsUI()
    {
        shortcuts = ShortcutLayer.CreateBuilder()
            .AddShortcut(Keys.E, ModifierKeys.None.WithControl(), tryFireEMP)
            .Build();
    }

    public void Initialize(GameInstance game, ShortcutCapturer shortcutCapturer)
    {
        gameInstance = game;
        this.shortcutCapturer = shortcutCapturer;
        shortcutCapturer.AddLayer(shortcuts);
        coreStats = game.State.Enumerate<ICoreStats>().SingleOrDefault();
        Health.SetFromSource(new CoreHealthStats(
            MaxHealth: coreStats?.MaxHealth ?? HitPoints.Zero,
            HealthAtWaveStart: coreStats?.CurrentHealth ?? HitPoints.Zero,
            CurrentHealth: coreStats?.CurrentHealth ?? HitPoints.Zero));
        updateBindings();

        game.Meta.Events.Subscribe<WaveScheduled>(this);
        game.Meta.Events.Subscribe<WaveStarted>(this);
        game.Meta.Events.Subscribe<WaveEnded>(this);
    }

    public void Terminate()
    {
        gameInstance?.State.Meta.Events.Unsubscribe<WaveScheduled>(this);
        gameInstance?.State.Meta.Events.Unsubscribe<WaveStarted>(this);
        gameInstance?.State.Meta.Events.Unsubscribe<WaveEnded>(this);
        shortcutCapturer?.RemoveLayer(shortcuts);
    }

    public void HandleEvent(WaveScheduled @event)
    {
        upcomingWaveInfo = new UpcomingWaveInfo(@event.WaveName, @event.SpawnStart, @event.CanSummonNow);
    }

    public void HandleEvent(WaveStarted @event)
    {
        Health.SetFromSource(Health.Value with
        {
            HealthAtWaveStart = coreStats?.CurrentHealth ?? Health.Value.HealthAtWaveStart
        });
        CurrentPhase.SetFromSource(GamePhase.InWave);
    }

    public void HandleEvent(WaveEnded @event)
    {
        CurrentPhase.SetFromSource(GamePhase.BetweenWaves);
    }

    private void tryFireEMP()
    {
        if (coreStats?.EMPStatus == EMPStatus.Ready)
        {
            FireEMP();
        }
    }

    public void FireEMP()
    {
        if (coreStats is not null)
        {
            gameInstance?.Request(FireEmergencyEMP.Request, coreStats.Object);
        }
    }

    public void SkipWaveTimer()
    {
        gameInstance?.Request(Game.Simulation.GameLoop.SkipWaveTimer.Request);
    }

    public void Update()
    {
        updateBindings();
    }

    private void updateBindings()
    {
        Visible.SetFromSource(coreStats is not null);
        Health.SetFromSource(Health.Value with
        {
            MaxHealth = coreStats?.MaxHealth ?? Health.Value.MaxHealth,
            CurrentHealth = coreStats?.CurrentHealth ?? Health.Value.CurrentHealth
        });
        EMPAvailable.SetFromSource(coreStats?.EMPStatus == EMPStatus.Ready);
        UpcomingWave.SetFromSource(upcomingWaveInfo is null || gameInstance is null
            ? null
            : upcomingWaveInfo.Value.ToCountdown(gameInstance.State.Time));
    }

    public readonly record struct CoreHealthStats(
        HitPoints MaxHealth, HitPoints HealthAtWaveStart, HitPoints CurrentHealth);

    private readonly record struct UpcomingWaveInfo(string Name, Instant? StartTime, Func<bool> CanSummonNow)
    {
        public UpcomingWaveCountdown ToCountdown(Instant currentTime) => new(
            Name,
            StartTime is null ? null : TimeSpan.Max(TimeSpan.Zero, StartTime.Value - currentTime),
            CanSummonNow());
    }

    public readonly record struct UpcomingWaveCountdown(string Name, TimeSpan? TimeLeft, bool CanSkip);

    public enum GamePhase
    {
        BetweenWaves,
        InWave
    }
}

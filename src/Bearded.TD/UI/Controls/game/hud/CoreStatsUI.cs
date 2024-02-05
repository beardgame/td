using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Shared.Events;
using Bearded.TD.UI.Shortcuts;
using Bearded.TD.Utilities;
using Bearded.UI.EventArgs;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.UI.Controls;

sealed class CoreStatsUI : IListener<WaveStarted>
{
    /*
     * Note: this UI is currently very specific to showing the statistics of the core.
     * However, it is totally possible to make this system more generic and fetch _any_ object from a specific interface
     * list and select which is the most important to draw. To keep things simple for now though, just core stats are
     * considered.
     */

    private readonly ShortcutLayer shortcuts;

    private GameInstance? gameInstance;
    private ShortcutCapturer? shortcutCapturer;
    private ICoreStats? coreStats;

    public Binding<bool> Visible { get; } = new();
    public Binding<CoreHealthStats> Health { get; } = new();
    public Binding<bool> EMPAvailable { get; } = new();

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
        game.State.Meta.Events.Subscribe(this);
    }

    public void Terminate()
    {
        gameInstance?.State.Meta.Events.Unsubscribe(this);
        shortcutCapturer?.RemoveLayer(shortcuts);
    }

    public void HandleEvent(WaveStarted @event)
    {
        Health.SetFromSource(Health.Value with
        {
            HealthAtWaveStart = coreStats?.CurrentHealth ?? Health.Value.HealthAtWaveStart
        });
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
        coreStats?.FireEMP();
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
    }

    public readonly record struct CoreHealthStats(
        HitPoints MaxHealth, HitPoints HealthAtWaveStart, HitPoints CurrentHealth);
}

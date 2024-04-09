using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class StatisticsSideBar : IListener<WaveReportCreated>
{
    private readonly Binding<WaveReport?> lastWaveReport = new();
    public IReadonlyBinding<WaveReport?> LastWaveReport => lastWaveReport;

    private readonly Binding<bool> waveReportVisible = new();
    public IReadonlyBinding<bool> WaveReportVisible => waveReportVisible;

    public IReadonlyBinding<bool> WaveReportButtonEnabled => lastWaveReport.Transform(r => r != null);
    public GameInstance Game { get; private set; } = null!;

    public void Initialize(GameInstance game)
    {
        Game = game;
        game.State.Meta.Events.Subscribe(this);

        // for debugging
        lastWaveReport.SetFromSource(WaveReport.Empty);
    }

    public void HandleEvent(WaveReportCreated e)
    {
        lastWaveReport.SetFromSource(e.Report);
        waveReportVisible.SetFromSource(true);
    }

    public void CloseWaveReport()
    {
        waveReportVisible.SetFromSource(false);
    }

    public void OpenWaveReport()
    {
        waveReportVisible.SetFromSource(true);
    }
}

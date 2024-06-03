using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.Game.Simulation.Statistics.Data;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities.Linq;

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
        lastWaveReport.SetFromSource(WaveReport.Create(dummyTowerDataForWaveReport()));
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

    private static IEnumerable<TowerStatistics> dummyTowerDataForWaveReport()
    {
        var random = new Random();
        var damageTypes = Bearded.Utilities.Linq.Extensions.RandomSubset([
            DamageType.Kinetic, DamageType.Fire, DamageType.Lightning,
            DamageType.Energy, DamageType.Frost, DamageType.Alchemy,
        ], random.Next(2, 5), random);

        return Enumerable.Range(1, 10)
            .Select(_ => new TowerStatistics(
                new GameObject(null, default, default),
                [
                    ..damageTypes.RandomSubset(random.Next(1, 3), random).Select(t =>
                    {
                        var efficiency = random.NextSingle();
                        var damage = new UntypedDamage(500.HitPoints() * random.NextSingle());
                        var data = new AccumulatedDamage(damage * efficiency, damage);
                        return new TypedAccumulatedDamage(t, data);
                    }),
                ]
            ));
    }
}

using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Shared.Events;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Statistics;

sealed class StatisticCollector : Component, ISyncable
{
    private double totalDamage;
    private long totalKills;

    private double currentWaveDamage;
    private long currentWaveKills;

    private double previousWaveDamage;
    private long previousWaveKills;
    private IListener<WaveStarted>? waveListener;

    protected override void OnAdded()
    {
        Events.Subscribe(Listener.ForEvent<CausedDamage>(e =>
        {
            totalDamage += e.Result.TotalExactDamage.Amount.NumericValue;
            currentWaveDamage += e.Result.TotalExactDamage.Amount.NumericValue;
        }));
        Events.Subscribe(Listener.ForEvent<CausedKill>(_ =>
        {
            totalKills++;
            currentWaveKills++;
        }));
        Events.Subscribe(Listener.ForEvent<ObjectDeleting>(_ => unsubscribe()));
        ReportAggregator.Register(Events, new StatisticsReport(this));
    }

    public override void Activate()
    {
        base.Activate();

        waveListener = Listener.ForEvent<WaveStarted>(_ =>
        {
            (previousWaveDamage, currentWaveDamage) = (currentWaveDamage, 0);
            (previousWaveKills, currentWaveKills) = (currentWaveKills, 0);
        });
        Owner.Game.Meta.Events.Subscribe(waveListener);
    }

    public override void OnRemoved()
    {
        unsubscribe();
        base.OnRemoved();
    }

    private void unsubscribe()
    {
        if (waveListener != null)
        {
            Owner.Game.Meta.Events.Unsubscribe(waveListener);
        }
    }

    public override void Update(TimeSpan elapsedTime) {}

    public IStateToSync GetCurrentStateToSync() => new StatisticCollectorStateToSync(this);

    private sealed class StatisticCollectorStateToSync : IStateToSync
    {
        private readonly StatisticCollector source;
        private double totalDamage;
        private long totalKills;

        public StatisticCollectorStateToSync(StatisticCollector source)
        {
            this.source = source;
            totalDamage = source.totalDamage;
            totalKills = source.totalKills;
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref totalDamage);
            stream.Serialize(ref totalKills);
        }

        public void Apply()
        {
            source.totalDamage = totalDamage;
            source.totalKills = totalKills;
        }
    }

    private sealed class StatisticsReport : IStatisticsReport
    {
        public ReportType Type => ReportType.Effectivity;

        public long TotalDamage => (long) source.totalDamage;
        public long TotalKills => source.totalKills;
        public long CurrentWaveDamage => (long) source.currentWaveDamage;
        public long CurrentWaveKills => source.currentWaveKills;
        public long PreviousWaveDamage => (long) source.previousWaveDamage;
        public long PreviousWaveKills => source.previousWaveKills;

        private readonly StatisticCollector source;

        public StatisticsReport(StatisticCollector source)
        {
            this.source = source;
        }
    }
}

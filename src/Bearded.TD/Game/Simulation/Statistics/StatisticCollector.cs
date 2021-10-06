using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Rendering;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Statistics
{
    sealed class StatisticCollector<T> : Component<T>, ISyncable where T : GameObject
    {
        private long totalDamage;
        private long totalKills;

        private long currentWaveDamage;
        private long currentWaveKills;

        private long previousWaveDamage;
        private long previousWaveKills;

        protected override void OnAdded()
        {
            Events.Subscribe(Listener.ForEvent<AttributedDamage>(e =>
            {
                totalDamage += e.Result.Damage.Amount.NumericValue;
                currentWaveDamage += e.Result.Damage.Amount.NumericValue;
            }));
            Events.Subscribe(Listener.ForEvent<AttributedKill>(_ =>
            {
                totalKills++;
                currentWaveKills++;
            }));
            Owner.Game.Meta.Events.Subscribe(Listener.ForEvent<WaveStarted>(_ =>
            {
                (previousWaveDamage, currentWaveDamage) = (currentWaveDamage, 0);
                (previousWaveKills, currentWaveKills) = (currentWaveKills, 0);
            }));
            ReportAggregator.Register(Events, new StatisticsReport(this));
        }

        public override void Update(TimeSpan elapsedTime) {}

        public override void Draw(CoreDrawers drawers) {}

        public IStateToSync GetCurrentStateToSync() => new StatisticCollectorStateToSync(this);

        private sealed class StatisticCollectorStateToSync : IStateToSync
        {
            private readonly StatisticCollector<T> source;
            private long totalDamage;
            private long totalKills;

            public StatisticCollectorStateToSync(StatisticCollector<T> source)
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

            public long TotalDamage => source.totalDamage;
            public long TotalKills => source.totalKills;
            public long CurrentWaveDamage => source.currentWaveDamage;
            public long CurrentWaveKills => source.currentWaveKills;
            public long PreviousWaveDamage => source.previousWaveDamage;
            public long PreviousWaveKills => source.previousWaveKills;

            private readonly StatisticCollector<T> source;

            public StatisticsReport(StatisticCollector<T> source)
            {
                this.source = source;
            }
        }
    }
}

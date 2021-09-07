using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Rendering;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Statistics
{
    sealed class StatisticCollector<T> : IComponent<T>, ISyncable where T : GameObject
    {
        private long totalDamage;
        private long totalKills;

        private long currentWaveDamage;
        private long currentWaveKills;

        private long previousWaveDamage;
        private long previousWaveKills;

        public void OnAdded(T owner, ComponentEvents events)
        {
            events.Subscribe(Listener.ForEvent<CausedDamage>(e =>
            {
                totalDamage += e.Result.Damage.Amount.NumericValue;
                currentWaveDamage += e.Result.Damage.Amount.NumericValue;
            }));
            events.Subscribe(Listener.ForEvent<CausedKill>(_ =>
            {
                totalKills++;
                currentWaveKills++;
            }));
            owner.Game.Meta.Events.Subscribe(Listener.ForEvent<WaveStarted>(_ =>
            {
                (previousWaveDamage, currentWaveDamage) = (currentWaveDamage, 0);
                (previousWaveKills, currentWaveKills) = (currentWaveKills, 0);
            }));
            ReportAggregator.Register(events, new StatisticsReport(this));
        }

        public void Update(TimeSpan elapsedTime) {}

        public void Draw(CoreDrawers drawers) {}

        public bool CanApplyUpgradeEffect(IUpgradeEffect effect) => false;

        public void ApplyUpgradeEffect(IUpgradeEffect effect) { }

        public bool RemoveUpgradeEffect(IUpgradeEffect effect) => false;

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

using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Statistics;

sealed class StatisticCollector : Component, ISyncable
{
    private double totalDamage;
    private long totalKills;

    protected override void OnAdded()
    {
        Events.Subscribe(Listener.ForEvent<CausedDamage>(e =>
        {
            totalDamage += e.Result.TotalExactDamage.Amount.NumericValue;
        }));
        Events.Subscribe(Listener.ForEvent<CausedKill>(_ =>
        {
            totalKills++;
        }));
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
}

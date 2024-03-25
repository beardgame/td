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

    protected override void OnAdded() {}

    public override void Activate()
    {
        if (Owner.TryGetSingleComponent<IIdProvider>(out var idProvider))
        {
            Events.Subscribe(Listener.ForEvent<CausedDamage>(e => { Owner.Game.Statistics.RegisterDamage(idProvider.Id, e.Result); }));
        }
    }

    public override void Update(TimeSpan elapsedTime) {}

    public IStateToSync GetCurrentStateToSync() => new StatisticCollectorStateToSync(this);

    private sealed class StatisticCollectorStateToSync : IStateToSync
    {
        private readonly StatisticCollector source;
        private double totalDamage;

        public StatisticCollectorStateToSync(StatisticCollector source)
        {
            this.source = source;
            totalDamage = source.totalDamage;
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref totalDamage);
        }

        public void Apply()
        {
            source.totalDamage = totalDamage;
        }
    }
}

using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Statistics;

sealed class StatisticCollector : Component
{
    protected override void OnAdded() {}

    public override void Activate()
    {
        if (Owner.TryGetSingleComponent<IIdProvider>(out var idProvider))
        {
            Events.Subscribe(Listener.ForEvent<CausedDamage>(e =>
            {
                Owner.Game.Statistics.RegisterDamage(idProvider.Id, e.Result);
            }));
        }
    }

    public override void Update(TimeSpan elapsedTime) {}
}

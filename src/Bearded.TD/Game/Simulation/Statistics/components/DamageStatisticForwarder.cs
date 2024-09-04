using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Statistics;

sealed class DamageStatisticForwarder : Component
{
    protected override void OnAdded() {}

    public override void Activate()
    {
        Events.Subscribe<CausedDamage>(e => Owner.Game.Statistics.RegisterDamage(Owner, e.Result));
    }

    public override void Update(TimeSpan elapsedTime) {}
}

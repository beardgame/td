using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings.Veterancy;

[Component("gainXpOnDamage")]
sealed class GainXpOnDamage : Component, IListener<CausedDamage>
{
    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(CausedDamage @event)
    {
        var damage = @event.Result.TotalExactDamage;

        if (damage.Amount == HitPoints.Zero) return;

        Events.Send(new GainXp(toXp(damage.Amount)));
    }

    private static Experience toXp(HitPoints hitPoints) => hitPoints.NumericValue.Xp();
}

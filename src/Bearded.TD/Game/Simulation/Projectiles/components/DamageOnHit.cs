using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("damageOnHit")]
sealed class DamageOnHit : Component<DamageOnHit.IParameters>, IListener<HitEnemy>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        DamageType? DamageType { get; }
    }

    public DamageOnHit(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public void HandleEvent(HitEnemy @event)
    {
        var damageDone = Owner.TryGetProperty<UntypedDamage>(out var damage)
            && DamageExecutor.FromObject(Owner)
                .TryDoDamage(@event.Enemy, damage.Typed(Parameters.DamageType ?? DamageType.Kinetic));
        DebugAssert.State.Satisfies(damageDone);
    }

    public override void Update(TimeSpan elapsedTime) { }
}

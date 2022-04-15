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
        [Modifiable(Type = AttributeType.Damage)]
        HitPoints Damage { get; }

        DamageType? Type { get; }
    }

    public DamageOnHit(IParameters parameters) : base(parameters) {}

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
        var damage = new DamageInfo(Parameters.Damage, Parameters.Type ?? DamageType.Kinetic);
        var damageDone = DamageExecutor.FromObject(Owner).TryDoDamage(@event.Enemy, damage);
        DebugAssert.State.Satisfies(damageDone);
    }

    public override void Update(TimeSpan elapsedTime) { }
}

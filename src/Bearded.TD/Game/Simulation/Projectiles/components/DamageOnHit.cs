using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("damageOnHit")]
sealed class DamageOnHit : Component<DamageOnHit.IParameters>, IListener<TouchObject>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        DamageType? DamageType { get; }

        [Modifiable(defaultValue: 1.0f)]
        float FractionOfBaseDamage { get; }

        bool ExcludeBuildings { get; }
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

    public void HandleEvent(TouchObject @event)
    {
        if (Parameters.ExcludeBuildings && @event.Object.TryGetSingleComponent<IBuildingStateProvider>(out _))
            return;

        _ = Owner.TryGetProperty<UntypedDamage>(out var damage)
            && DamageExecutor.FromObject(Owner).TryDoDamage(
                @event.Object,
                (damage * Parameters.FractionOfBaseDamage).Typed(Parameters.DamageType ?? DamageType.Force),
                Hit.FromImpact(@event.Impact));
    }

    public override void Update(TimeSpan elapsedTime) { }
}

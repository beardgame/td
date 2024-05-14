using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("splashDamageOnKilled")]
sealed class SplashDamageOnKilled : Component<SplashDamageOnKilled.IParameters>, IListener<ObjectKilled>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.SplashRange)]
        Unit Range { get; }

        [Modifiable(10)]
        UntypedDamage Damage { get; }

        DamageType? DamageType { get; }
    }

    public SplashDamageOnKilled(IParameters parameters) : base(parameters) {}

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public void HandleEvent(ObjectKilled @event)
    {
        var damage = Parameters.Damage.Typed(Parameters.DamageType ?? DamageType.Kinetic);

        AreaOfEffect.Damage(
            Owner.Game,
            DamageExecutor.WithoutDamageSource(),
            damage,
            Owner.Position,
            Parameters.Range);
    }

    public override void Update(TimeSpan elapsedTime) { }
}

using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Elements.Phenomena;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("igniteNearbyObjectsOnKilled")]
sealed class IgniteNearbyObjectsOnKilled : Component<IgniteNearbyObjectsOnKilled.IParameters>, IListener<ObjectKilled>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.SplashRange)]
        Unit Range { get; }

        [Modifiable(1)]
        TimeSpan Duration { get; }

        [Modifiable(30)]
        UntypedDamagePerSecond DamagePerSecond { get; }
    }

    public IgniteNearbyObjectsOnKilled(IParameters parameters) : base(parameters) {}

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
        AreaOfEffect.ApplyStatusEffect(
            Owner.Game,
            new OnFire.Effect(Parameters.DamagePerSecond, null, Parameters.Duration),
            Owner.Position,
            Parameters.Range);
    }

    public override void Update(TimeSpan elapsedTime) { }
}

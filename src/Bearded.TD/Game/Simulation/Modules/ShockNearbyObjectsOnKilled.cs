using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Elements.Phenomena;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("shockNearbyObjectsOnKilled")]
sealed class ShockNearbyObjectsOnKilled : Component<ShockNearbyObjectsOnKilled.IParameters>, IListener<ObjectKilled>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.SplashRange)]
        Unit Range { get; }

        [Modifiable(0.5)]
        TimeSpan Duration { get; }

        [Modifiable(0.5)]
        double MovementPenalty { get; }
    }

    public ShockNearbyObjectsOnKilled(IParameters parameters) : base(parameters) {}

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
            new Shocked.Effect(Parameters.MovementPenalty, Parameters.Duration),
            Owner.Position,
            Parameters.Range);
    }

    public override void Update(TimeSpan elapsedTime) { }
}

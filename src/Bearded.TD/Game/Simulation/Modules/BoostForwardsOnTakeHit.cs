using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("boostForwardsOnTakeHit")]
sealed class BoostForwardsOnTakeHit : Component<BoostForwardsOnTakeHit.IParameters>, IListener<TakeHit>
{
    private IPhysics physics = null!;
    private Instant nextPossibleBoostTime;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        Speed Impulse { get; }
        TimeSpan Cooldown { get; }
        IGameObjectBlueprint? SpawnObject { get; }
    }

    public BoostForwardsOnTakeHit(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
        ComponentDependencies.Depend<IPhysics>(Owner, Events, p => physics = p);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(TakeHit @event)
    {
        tryBoostForward();
    }

    private void tryBoostForward()
    {
        if (nextPossibleBoostTime > Owner.Game.Time)
            return;

        boostForward();

        nextPossibleBoostTime = Owner.Game.Time + Parameters.Cooldown;
    }

    private void boostForward()
    {
        var velocity = physics.Velocity;

        var vector = velocity.NumericValue.NormalizedSafe();

        var impulse = vector * Parameters.Impulse;

        physics.ApplyVelocityImpulse(impulse);

        if (Parameters.SpawnObject != null)
        {
            var obj = GameObjectFactory
                .CreateFromBlueprintWithDefaultRenderer(Parameters.SpawnObject, Owner, Owner.Position, Direction2.Of(vector.Xy));
            obj.AddComponent(new InheritDirection());
            Owner.Game.Add(obj);
        }
    }
}

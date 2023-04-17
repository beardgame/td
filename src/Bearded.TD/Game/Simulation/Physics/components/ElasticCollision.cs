using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("elasticCollision")]
sealed class ElasticCollision : Component<ElasticCollision.IParameters>, IListener<HitObject>, IListener<HitLevel>
{
    private IPhysics physics = null!;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        float Normal { get; }
        [Modifiable(1)]
        float Tangent { get; }
    }

    public ElasticCollision(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe<HitObject>(this);
        Events.Subscribe<HitLevel>(this);
        ComponentDependencies.Depend<IPhysics>(Owner, Events, p => physics = p);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(HitObject @event)
    {
        onHit(@event.Impact.SurfaceNormal);
    }

    public void HandleEvent(HitLevel @event)
    {
        onHit(@event.Info.SurfaceNormal);
    }

    private void onHit(Difference3 surfaceNormal)
    {
        var normal = surfaceNormal.NumericValue.NormalizedSafe();

        var velocityIn = physics.Velocity;

        var dotWithVelocityOutMagnitude = Vector3.Dot(normal, -velocityIn.NumericValue);

        var normalVelocityOut = new Velocity3(normal * dotWithVelocityOutMagnitude);
        var tangentVelocity = velocityIn + normalVelocityOut;

        var velocityOut = normalVelocityOut * Parameters.Normal + tangentVelocity * Parameters.Tangent;

        physics.ApplyVelocityImpulse(velocityOut - velocityIn);
    }
}


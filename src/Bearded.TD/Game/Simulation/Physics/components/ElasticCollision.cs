using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("elasticCollision")]
sealed class ElasticCollision : Component<ElasticCollision.IParameters>, IListener<CollideWithObject>, IListener<CollideWithLevel>
{
    private IPhysics physics = null!;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        float Normal { get; }
        [Modifiable(1)]
        float Tangent { get; }

        bool ExcludeWalls { get; }
        bool ExcludeFloor { get; }
        bool ExcludeObjects { get; }
    }

    public ElasticCollision(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        if (Parameters is not { ExcludeFloor: true, ExcludeWalls: true })
            Events.Subscribe<CollideWithLevel>(this);

        if (Parameters is not { ExcludeObjects: true })
            Events.Subscribe<CollideWithObject>(this);

        ComponentDependencies.Depend<IPhysics>(Owner, Events, p => physics = p);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(CollideWithObject @event)
    {
        onHit(@event.Impact.SurfaceNormal);
    }

    public void HandleEvent(CollideWithLevel e)
    {
        var isFloor = e.Info.SurfaceNormal == new Difference3(0, 0, 1);

        var excludeHit = isFloor ? Parameters.ExcludeFloor : Parameters.ExcludeWalls;

        if (!excludeHit)
            onHit(e.Info.SurfaceNormal);
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


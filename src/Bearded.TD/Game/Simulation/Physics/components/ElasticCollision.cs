using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.Game.Simulation.Physics.ElasticCollision;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("elasticCollision")]
sealed class ElasticCollision(IParameters parameters) : Component<IParameters>(parameters),
        IPreviewListener<CollidingWithObject>, IListener<CollidedWithObject>, IListener<CollidedWithLevel>
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

    protected override void OnAdded()
    {
        if (Parameters is not { ExcludeObjects: true })
        {
            Events.Subscribe(this);
            Events.Subscribe<CollidedWithObject>(this);
        }

        if (Parameters is not { ExcludeFloor: true, ExcludeWalls: true })
            Events.Subscribe<CollidedWithLevel>(this);

        ComponentDependencies.Depend<IPhysics>(Owner, Events, p => physics = p);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
        Events.Unsubscribe<CollidedWithObject>(this);
        Events.Unsubscribe<CollidedWithLevel>(this);
    }

    public void PreviewEvent(ref CollidingWithObject e)
    {
        e = e with { Collided = true };
    }

    public void HandleEvent(CollidedWithObject @event)
    {
        onHit(@event.Impact.SurfaceNormal);
    }

    public void HandleEvent(CollidedWithLevel e)
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


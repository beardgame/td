using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("spawnObjectOnHit")]
sealed class SpawnObjectOnHit
    : Component<SpawnObjectOnHit.IParameters>, IListener<CollidedWithLevel>, IListener<ObjectHit>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        IGameObjectBlueprint Object { get; }

        bool OnHitLevel { get; }

        bool OnHitEnemy { get; }
    }


    public SpawnObjectOnHit(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        if (Parameters.OnHitEnemy)
        {
            Events.Subscribe<ObjectHit>(this);
        }

        if (Parameters.OnHitLevel)
        {
            Events.Subscribe<CollidedWithLevel>(this);
        }
    }

    protected override void OnRemovedInternal()
    {
        if (Parameters.OnHitEnemy)
        {
            Events.Unsubscribe<ObjectHit>(this);
        }

        if (Parameters.OnHitLevel)
        {
            Events.Unsubscribe<CollidedWithLevel>(this);
        }
    }

    public void HandleEvent(CollidedWithLevel e)
    {
        onHit(e.Info, null);
    }


    public void HandleEvent(ObjectHit e)
    {
        if (e.Hit.Impact is not { } impact)
        {
            DebugAssert.State.IsInvalid();
            return;
        }

        onHit(impact, e.Object);
    }

    private void onHit(Impact hit, GameObject? hitObj)
    {
        var obj = GameObjectFactory
            .CreateFromBlueprintWithDefaultRenderer(Parameters.Object, Owner, hit.Point, Direction2.Zero);

        obj.AddComponent(new Property<Impact>(hit));

        if (hitObj != null)
            obj.AddComponent(Property.From(hitObj.AsHitObject()));

        Owner.Game.Add(obj);
    }
}

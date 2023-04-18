using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("spawnObjectOnHit")]
sealed class SpawnObjectOnHit
    : Component<SpawnObjectOnHit.IParameters>, IListener<CollideWithLevel>, IListener<CollideWithObject>
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
            Events.Subscribe<CollideWithObject>(this);
        }

        if (Parameters.OnHitLevel)
        {
            Events.Subscribe<CollideWithLevel>(this);
        }
    }

    public override void OnRemoved()
    {
        if (Parameters.OnHitEnemy)
        {
            Events.Unsubscribe<CollideWithObject>(this);
        }

        if (Parameters.OnHitLevel)
        {
            Events.Unsubscribe<CollideWithLevel>(this);
        }
    }

    public void HandleEvent(CollideWithLevel e)
    {
        onHit(e.Info);
    }

    public void HandleEvent(CollideWithObject e)
    {
        onHit(e.Impact);
    }

    private void onHit(Impact hit)
    {
        var obj = GameObjectFactory
            .CreateFromBlueprintWithDefaultRenderer(Parameters.Object, Owner, hit.Point, Direction2.Zero);
        obj.AddComponent(new Property<Impact>(hit));
        Owner.Game.Add(obj);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}

using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("spawnObjectOnHit")]
sealed class SpawnObjectOnHit
    : Component<SpawnObjectOnHit.IParameters>, IListener<HitLevel>, IListener<HitEnemy>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        IComponentOwnerBlueprint Object { get; }

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
            Events.Subscribe<HitEnemy>(this);
        }

        if (Parameters.OnHitLevel)
        {
            Events.Subscribe<HitLevel>(this);
        }
    }

    public override void OnRemoved()
    {
        if (Parameters.OnHitEnemy)
        {
            Events.Unsubscribe<HitEnemy>(this);
        }

        if (Parameters.OnHitLevel)
        {
            Events.Unsubscribe<HitLevel>(this);
        }
    }

    public void HandleEvent(HitLevel e)
    {
        onHit(e.Info);
    }

    public void HandleEvent(HitEnemy e)
    {
        onHit(e.Info);
    }

    private void onHit(HitInfo hit)
    {
        var obj = GameObjectFactory
            .CreateFromBlueprintWithDefaultRenderer(Parameters.Object, Owner, Owner.Position, Direction2.Zero);
        obj.AddComponent(new Property<HitInfo>(hit));
        Owner.Game.Add(obj);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}

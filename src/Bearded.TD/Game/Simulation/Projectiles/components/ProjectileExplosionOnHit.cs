using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("projectileExplosionOnHit")]
sealed class ProjectileExplosionOnHit
    : Component<ProjectileExplosionOnHit.IParameters>, IListener<CollidedWithLevel>, IListener<ObjectHit>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        IGameObjectBlueprint Projectile { get; }

        bool OnHitLevel { get; }

        bool OnHitEnemy { get; }

        [Modifiable(7)]
        int MinProjectileNumber { get; }
        [Modifiable(10)]
        int MaxProjectileNumber { get; }

        [Modifiable(1)]
        Speed RandomVelocity { get; }

        [Modifiable(1)]
        float DamageFactor { get; }
    }

    public ProjectileExplosionOnHit(IParameters parameters) : base(parameters)
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

    public override void OnRemoved()
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

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(CollidedWithLevel e)
    {
        if (!Owner.TryGetProperty<UntypedDamage>(out var damage))
        {
            DebugAssert.State.IsInvalid();
            return;
        }

        onHit(e.Info, damage);
    }

    public void HandleEvent(ObjectHit e)
    {
        if (e.Hit.Impact is not { } impact)
        {
            DebugAssert.State.IsInvalid();
            return;
        }

        onHit(impact, e.Hit.DamagePotential);
    }

    private void onHit(Impact hit, UntypedDamage damage)
    {

        var projectiles = ProjectileExplosion.CreateProjectilesForExplosion(
            Parameters.Projectile,
            Owner,
            damage * Parameters.DamageFactor,
            Parameters.MinProjectileNumber,
            Parameters.MaxProjectileNumber,
            Parameters.RandomVelocity);
        foreach (var projectile in projectiles)
        {
            projectile.AddComponent(new Property<Impact>(hit));
            Owner.Game.Add(projectile);
        }
    }
}
